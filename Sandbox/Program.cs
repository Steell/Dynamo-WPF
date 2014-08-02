using System;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NUnit.Framework;
using ObservableExtensions;

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

        public struct Connector<TNode, TMetaData>
        {
            public Connection<TNode, TMetaData> Start, End;

            public override string ToString()
            {
                return string.Format("{0}=====>{1}", Start.Node, End.Node);
            }
        }

        public struct Connection<TNode, TMetaData>
        {
            public TNode Node;
            public TMetaData PortIndex;
            public ConnectionType Type;

            public override string ToString()
            {
                return string.Format("({0} {1})", Node, Type);//, PortIndex);
            }
        }

        public enum ConnectionType
        {
            Input, Output
        }

        /// <summary>
        ///     Interactive model of a Graph that supports asynchronous creation of connectors.
        /// </summary>
        /// <typeparam name="TNode">Type of all nodes.</typeparam>
        /// <typeparam name="TMetaData">Type of data attached to all connections.</typeparam>
        public class GraphModel<TNode, TMetaData>
        {
            /// <summary>
            ///     Observable sequence of new connectors
            /// </summary>
            public IObservable<Connector<TNode, TMetaData>> ConnectorCreatedStream
            {
                get;
                private set;
            }

            /// <summary>
            ///     Observable sequence of the start of a new connection action
            /// </summary>
            public IObservable<Connection<TNode, TMetaData>> BeginNewConnectionStream
            {
                get;
                private set;
            }

            /// <summary>
            ///     Observable sequence of the end of a new connection action
            /// </summary>
            public IObservable<Connection<TNode, TMetaData>?> EndNewConnectionStream 
            { 
                get; 
                private set; 
            }

            public GraphModel(
                IObservable<Connection<TNode, TMetaData>> beginNewConnectionStream,
                IObservable<Connection<TNode, TMetaData>?> endNewConnectionStream)
            {
                BeginNewConnectionStream = beginNewConnectionStream;
                EndNewConnectionStream = endNewConnectionStream;

                ConnectorCreatedStream =
                    // Listen for a connection start event, then a connection end event
                    BeginNewConnectionStream.FollowedBy(
                        EndNewConnectionStream, NewConnectorData.Create)
                        .Switch()                      // Only track newest connection
                        .Where(c => c.IsValid())       // Ignore cancelled connections
                        .Select(c => c.ToConnector()); // Build connector from data
            }

            private struct NewConnectorData
            {
                private Connection<TNode, TMetaData> start;
                private Connection<TNode, TMetaData>? end;

                public static NewConnectorData Create(
                    Connection<TNode, TMetaData> start,
                    Connection<TNode, TMetaData>? end)
                {
                    return new NewConnectorData(start, end);
                }

                private NewConnectorData(
                    Connection<TNode, TMetaData> start, 
                    Connection<TNode, TMetaData>? end)
                    : this()
                {
                    this.start = start;
                    this.end = end;
                }

                public Connector<TNode, TMetaData> ToConnector()
                {
                    return start.Type == ConnectionType.Output
// ReSharper disable PossibleInvalidOperationException
                        ? new Connector<TNode, TMetaData> { Start = start, End = end.Value }
                        : new Connector<TNode, TMetaData> { Start = end.Value, End = start };
// ReSharper restore PossibleInvalidOperationException
                }

                public bool IsValid()
                {
                    return end.HasValue && start.Type != end.Value.Type;
                }
            }
        }

        /// <summary>
        ///     Interactive model of a Graph that supports asynchronous creation and deletion of 
        ///     connectors.
        /// </summary>
        /// <typeparam name="TNode">Type of all nodes.</typeparam>
        /// <typeparam name="TMetaData">Type of data attached to all connections.</typeparam>
        public class GraphModelWithDeletion<TNode, TMetaData> : GraphModel<TNode, TMetaData>
        {
            /// <summary>
            ///     Observable sequence of deleted connectors
            /// </summary>
            public IObservable<Connector<TNode, TMetaData>> ConnectorDeletedStream
            {
                get; 
                private set;
            }

            public GraphModelWithDeletion(
                IObservable<Connection<TNode, TMetaData>> beginNewConnectionStream,
                IObservable<Connection<TNode, TMetaData>?> endNewConnectionStream,
                IObservable<TNode> nodeDeletedStream,
                IObservable<Connector<TNode, TMetaData>> connectorDeletedStream)
                : base(beginNewConnectionStream, endNewConnectionStream)
            {
                // Observable sequence of "hanging" connectors
                var hangingConnectors =
                    // For each new connector...
                    ConnectorCreatedStream.SelectMany(
                        connector =>
                            nodeDeletedStream // ...listen for a node deletion...
                                
                                // ...where the node deleted was attached to the connector.
                                .Where(
                                    node => connector.Start.Node.Equals(node)
                                            || connector.End.Node.Equals(node))

                                .FirstAsync()           // Only listen for one occurence
                                .Select(_ => connector) // Produce the hanging connector.
                                
                                // Stop listening once the connector has been deleted.
                                .TakeUntil(
                                    connectorDeletedStream.Where(c => connector.Equals(c))));

                var validDeletions =
                    // Make sure we only broadcast deletions of connectors that actually exist
                    ConnectorCreatedStream.SelectMany(
                        connector => 
                            connectorDeletedStream.Where(
                                other => connector.Equals(other)));

                // Deleted connectors are both external deletions and hanging connectors caused by
                // node deletions.
                ConnectorDeletedStream = validDeletions.Merge(hangingConnectors);
            }
        }

        /// <summary>
        ///     Interactive model of a Graph that supports asynchronous creation, disconnection,
        ///     and reconnection of connectors.
        /// </summary>
        /// <typeparam name="TNode">Type of all nodes.</typeparam>
        /// <typeparam name="TMetaData">Type of data attached to all connections.</typeparam>
        public class GraphModelWithDisconnect<TNode, TMetaData> 
            : GraphModelWithDeletion<TNode, TMetaData>
        {
            public GraphModelWithDisconnect(
                IObservable<Connector<TNode, TMetaData>> disconnectStream,
                IObservable<Connection<TNode, TMetaData>> beginNewConnectionStream,
                IObservable<Connection<TNode, TMetaData>?> endNewConnectionStream, 
                IObservable<TNode> nodeDeletedStream)
                : base(
                    //New connections are also triggered from disconnects
                    beginNewConnectionStream.Merge(disconnectStream.Select(x => x.Start)),
                    endNewConnectionStream,
                    nodeDeletedStream,
                    //Disconnects trigger deletions
                    disconnectStream)
            { }
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
