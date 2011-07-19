using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;

namespace AsynMsmqSpike.Publisher {
	class Program {
		static void Main(string[] args) {
			const int count = 2000;
			const string queuePath = @".\private$\test_queue";
			if(!MessageQueue.Exists(queuePath)) {
				MessageQueue.Create(queuePath, true);
			}

			var queue = new MessageQueue(queuePath);
			queue.Formatter = new XmlMessageFormatter();

			var messagebodys = Enumerable.Range(0, count).Select(x => string.Format("Hello World {0}", x));

			foreach(var messagebody in messagebodys) {
				queue.Send(messagebody, MessageQueueTransactionType.Single);
				Console.Write('.');
			}

			Console.WriteLine("Sent {0} messages. Press enter to terminate!", count);
			Console.ReadLine();
		}
	}
}
