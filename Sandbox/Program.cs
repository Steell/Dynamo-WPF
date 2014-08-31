using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Dynamo.UI.Models;
using NUnit.Framework;
using ObservableExtensions;
using ObservableExtensions.Experimental;

namespace Sandbox
{
    static class Program
    {
        /*
        public struct Port
        {
            public string Name;
        }

        public struct Node
        {
            public string Id;
            public Port[] InputPorts;
            public Port[] OutputPorts;

            public override string ToString()
            {
                return string.Format("(Node: Id={0})", Id);
            }
        }
         * */

        struct Butt
        {
            public object Hole;
            public Butt(object hole) : this()
            {
                Hole = hole;
            }
        }

        [Test]
        public static void StructEquality()
        {
            var o = new object();
            var a = new Butt(o);
            var b = new Butt(o);

            Assert.IsTrue(a.Equals(b));
            Assert.IsFalse(a.Equals(new Butt(new object())));
        }


        [Test]
        public static void CanCreateConnector()
        {
            var nodes =
                new[] {0, 1, 2}
                    .Select(x => x.ToString(CultureInfo.InvariantCulture))
                    .ToArray();

            var startConnection = new Subject<Connection<string, int>>();
            var endConnection = new Subject<Connection<string, int>?>();
            var disconnect = new Subject<Connector<string, int>>();
            var deleteNode = new Subject<string>();

            startConnection.Dump("StartConnection");
            endConnection.Dump("EndConnection");
            disconnect.Dump("Disconnect");
            deleteNode.Dump("DeleteNode");

            var workspace =
                new GraphModelWithDisconnect<string, int>(
                    disconnect, startConnection, endConnection, deleteNode);

            workspace.ConnectorCreatedStream.Dump("  ConnectorCreated");
            workspace.ConnectorDeletedStream.Dump("  ConnectorDeleted");
            workspace.BeginNewConnectionStream.Dump("  ConnectionStarted");

            startConnection.OnNext(
                new Connection<string, int> { Node = nodes[1], Type = ConnectionType.Output });
            startConnection.OnNext(
                new Connection<string, int> { Node = nodes[0], Type = ConnectionType.Output });
            endConnection.OnNext(null);
            startConnection.OnNext(
                new Connection<string, int> { Node = nodes[1], Type = ConnectionType.Input });
            endConnection.OnNext(
                new Connection<string, int> { Node = nodes[0], Type = ConnectionType.Output });
            
            disconnect.OnNext(
                new Connector<string, int>
                {
                    Start = new Connection<string, int>
                    {
                        Node = nodes[0], Type = ConnectionType.Output
                    },
                    End = new Connection<string, int>
                    {
                        Node = nodes[1], Type = ConnectionType.Input
                    }
                });

            endConnection.OnNext(new Connection<string, int>
            {
                Node = nodes[2], Type = ConnectionType.Input
            });
            startConnection.OnNext(new Connection<string, int>
            {
                Node = nodes[0], Type = ConnectionType.Output
            });
            endConnection.OnNext(new Connection<string, int>
            {
                Node = nodes[1], Type = ConnectionType.Input
            });

            deleteNode.OnNext(nodes[0]);
            
            Assert.Pass("Success");
        }

        [Test]
        public static void DisposalFiresOnCompleted()
        {
            Observable
                .Return(0)
                .Feedback(Observable.Return, i => Observable.Return(i + 1))
                .Take(10)
                .Dump("Count");
        }

        [Test]
        public static void ControlFlow()
        {
            var test = new Subject<bool>();
            var ifTrue = new Subject<string>();
            var ifFalse = new Subject<string>();

            var branching = test.IfThenElse(
                ifTrue.Do(x => Console.WriteLine("true-->{0}", x)), 
                ifFalse.Do(x => Console.WriteLine("false-->{0}", x)));

            branching.Dump("Out");
            
            test.OnNext(true);
            ifTrue.OnNext("true");
            ifFalse.OnNext("false");
            test.OnNext(false);
            ifTrue.OnNext("true2");
            test.OnNext(true);
        }
    }
}
