using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsynMsmqSpike.Subscriber {
	class Program {
		static void Main(string[] args) {
			var listener = new MsmqListener(@".\private$\test_queue");
			listener.MessageReceived += listener_MessageReceived;
			listener.Start();
			Console.WriteLine("Press enter to terminate!");
			Console.ReadLine();
			listener.Stop();
		}

		static void listener_MessageReceived(object sender, MessageEventArgs args) {
			Console.WriteLine("Received Message {0} with body {1}", args.Message.Id, args.Message.Body); 
		}
	}
}
