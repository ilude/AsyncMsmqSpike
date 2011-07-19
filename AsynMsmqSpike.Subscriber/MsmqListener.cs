using System;
using System.Diagnostics;
using System.Messaging;

namespace AsynMsmqSpike.Subscriber {
	public delegate void MessageReceivedEventHandler(object sender, MessageEventArgs args);

	public class MsmqListener {
		private bool listen;
		private Type[] types;
		private readonly MessageQueue queue;

		public event MessageReceivedEventHandler MessageReceived;

		public Type[] FormatterTypes {
			get { return types; }
			set { types = value; }
		}

		public MsmqListener(string queuePath) {
			if(!MessageQueue.Exists(queuePath)) {
				MessageQueue.Create(queuePath, true);
				Debug.WriteLine("Created queue {0}", queuePath);
			}
			else {
				Debug.WriteLine("Queue {0} exists", queuePath);
			}

			queue = new MessageQueue(queuePath);
		}

		public void Start() {
			listen = true;

			if(types != null && types.Length > 0) {
				// Using only the XmlMessageFormatter. You can use other formatters as well
				queue.Formatter = new XmlMessageFormatter(types);
			}
			else {
				queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
			}

			queue.PeekCompleted += OnPeekCompleted;
			queue.ReceiveCompleted += OnReceiveCompleted;

			StartListening();
		}

		public void Stop() {
			listen = false;
			queue.PeekCompleted -= OnPeekCompleted;
			queue.ReceiveCompleted -= OnReceiveCompleted;

		}

		private void StartListening() {
			if(!listen) {
				return;
			}

			// The MSMQ class does not have a BeginRecieve method that can take in a 
			// MSMQ transaction object. This is a workaround - we do a BeginPeek and then 
			// receive the message synchronously in a transaction.
			// Check documentation for more details
			if(queue.Transactional) {
				queue.BeginPeek();
				Debug.WriteLine("Peeking");
			}
			else {
				queue.BeginReceive();
				Debug.WriteLine("Receiving");
			}
		}

		private void OnPeekCompleted(object sender, PeekCompletedEventArgs e) {
			Debug.WriteLine("Peek Complete!");
			queue.EndPeek(e.AsyncResult);
			var transaction = new MessageQueueTransaction();
			try {
				transaction.Begin();
				var msg = queue.Receive(transaction);
				transaction.Commit();

				StartListening();

				FireRecieveEvent(msg);
			}
			catch(Exception ex) {
				if(transaction.Status == MessageQueueTransactionStatus.Pending)
					transaction.Abort();

				Debug.WriteLine("Peek Exception: {0}", ex.Message);
			}
		}

		private void FireRecieveEvent(Message message) {
			if(MessageReceived != null) {
				MessageReceived(this, new MessageEventArgs(message));
			}
		}

		private void OnReceiveCompleted(object sender, ReceiveCompletedEventArgs e) {
			Debug.WriteLine("Receive Complete!");
			var msg = queue.EndReceive(e.AsyncResult);

			StartListening();

			FireRecieveEvent(msg);
		}

	}

	public class MessageEventArgs : EventArgs {
		private readonly Message message;

		public Message Message {
			get { return message; }
		}

		public MessageEventArgs(Message body) {
			message = body;
		}
	}

}