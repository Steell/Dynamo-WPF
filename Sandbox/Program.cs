using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                return string.Format("{0}=====>{1}", Start, End);
            }
        }

        public struct Connection<TNode, TMetaData>
        {
            public TNode Node;
            public TMetaData PortIndex;

            public override string ToString()
            {
                return Node.ToString(); //string.Format("(Connection: Node={0})", Node);//, PortIndex);
            }
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
            public IObservable<Connector<TNode, TMetaData>> ConnectorCreatedStream { get; private set; }

            /// <summary>
            ///     Observable sequence of the start of a new connection action
            /// </summary>
            public IObservable<Connection<TNode, TMetaData>> BeginNewConnectionStream { get; private set; }

            /// <summary>
            ///     Observable sequence of the end of a new connection action
            /// </summary>
            public IObservable<Connection<TNode, TMetaData>?> EndNewConnectionStream { get; private set; }

            public GraphModel(
                IObservable<Connection<TNode, TMetaData>> beginNewConnectionStream,
                IObservable<Connection<TNode, TMetaData>?> endNewConnectionStream)
            {
                BeginNewConnectionStream = beginNewConnectionStream;
                EndNewConnectionStream = endNewConnectionStream;

                ConnectorCreatedStream =
                    // Listen for a connection start event
                    BeginNewConnectionStream

                        // Then, listen for something to end the connection in progress.
                        .Select(start => EndNewConnectionStream.Select(end => new { start, end }))

                        // When a new connection start comes in, stop tracking the old one
                        .Switch()

                        // Ignore cancelled connections
                        .Where(c => c.end.HasValue)

                        // Build connectors from the start and end
                        .Select(c => new Connector<TNode, TMetaData> { Start = c.start, End = c.end.Value });
            }
        }

        /// <summary>
        ///     Interactive model of a Graph that supports asynchronous creation and
        ///     deletion of connectors.
        /// </summary>
        /// <typeparam name="TNode">Type of all nodes.</typeparam>
        /// <typeparam name="TMetaData">Type of data attached to all connections.</typeparam>
        public class GraphModelWithDeletion<TNode, TMetaData> : GraphModel<TNode, TMetaData>
        {
            /// <summary>
            ///     Observable sequence of deleted connectors
            /// </summary>
            public IObservable<Connector<TNode, TMetaData>> ConnectorDeletedStream { get; private set; }

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
                            // ...listen for a node deletion...
                            nodeDeletedStream

                                // ...where the node deleted was attached to the connector.
                                .Where(
                                    node =>
                                        connector.Start.Node.Equals(node) || connector.End.Node.Equals(node))
                            
                                // Only take the first occurence (no need to delete the same connector twice)
                                .FirstAsync()
                                
                                // Produce the hanging connector.
                                .Select(_ => connector)
                                
                                // Stop listening once the connector has been deleted.
                                .TakeUntil(connectorDeletedStream.Where(c => connector.Equals(c))));

                // Deleted connectors are both external deletions and hanging connectors caused by node deletions.
                ConnectorDeletedStream = connectorDeletedStream.Merge(hangingConnectors);
            }
        }

        /// <summary>
        ///     Interactive model of a Graph that supports asynchronous creation, disconnection,
        ///     and reconnection of connectors.
        /// </summary>
        /// <typeparam name="TNode">Type of all nodes.</typeparam>
        /// <typeparam name="TMetaData">Type of data attached to all connections.</typeparam>
        public class GraphModelWithDisconnect<TNode, TMetaData> : GraphModelWithDeletion<TNode, TMetaData>
        {
            public GraphModelWithDisconnect(
                IObservable<Connector<TNode, TMetaData>> disconnectStream,
                IObservable<Connection<TNode, TMetaData>> beginNewConnectionStream,
                IObservable<Connection<TNode, TMetaData>?> endNewConnectionStream, 
                IObservable<TNode> nodeDeletedStream)
                : base(
                    beginNewConnectionStream.Merge(disconnectStream.Select(x => x.Start)),
                    endNewConnectionStream,
                    nodeDeletedStream,
                    disconnectStream)
            { }
        }

        [Test]
        public static void CanCreateConnector()
        {
            var nodes =
                new[] {0, 1, 2}
                    .Select(x => x.ToString())
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

            startConnection.OnNext(new Connection<string, int> { Node = nodes[1] });
            startConnection.OnNext(new Connection<string, int> { Node = nodes[0] });
            endConnection.OnNext(new Connection<string, int> { Node = nodes[1] });
            
            disconnect.OnNext(
                new Connector<string, int>
                {
                    Start = new Connection<string, int> { Node = nodes[0] },
                    End = new Connection<string, int> { Node = nodes[1] }
                });

            endConnection.OnNext(new Connection<string, int> { Node = nodes[2] });
            startConnection.OnNext(new Connection<string, int> { Node = nodes[0] });
            endConnection.OnNext(new Connection<string, int> { Node = nodes[1] });

            deleteNode.OnNext(nodes[0]);
            
            Assert.Pass("Success");
        }
    }
}
