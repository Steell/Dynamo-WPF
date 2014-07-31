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

        public class GraphModel<TNode, TMetaData>
        {
            public IObservable<Connector<TNode, TMetaData>> ConnectorCreatedStream { get; private set; }
            public IObservable<Connection<TNode, TMetaData>> BeginNewConnectionStream { get; private set; }
            public IObservable<Connection<TNode, TMetaData>?> EndNewConnectionStream { get; private set; }
            public IObservable<Connector<TNode, TMetaData>> ConnectorDeletedStream { get; private set; }

            public GraphModel(
                IObservable<Connection<TNode, TMetaData>> beginNewConnectionStream,
                IObservable<Connection<TNode, TMetaData>?> endNewConnectionStream,
                IObservable<TNode> nodeDeletedStream,
                IObservable<Connector<TNode, TMetaData>> connectorDeletedStream)
            {
                BeginNewConnectionStream = beginNewConnectionStream;
                EndNewConnectionStream = endNewConnectionStream;

                ConnectorCreatedStream = 
                    // Listen for one connection start event
                    BeginNewConnectionStream.FirstAsync()

                        // Then, listen for something to end the connection in progress.
                        .SelectMany(start => EndNewConnectionStream.Select(end => new { start, end }))

                        // Ignore cancelled connections
                        .Where(x => x.end.HasValue)

                        // Build connectors from the start and end
                        .Select(c => new Connector<TNode, TMetaData> { Start = c.start, End = c.end.Value })

                        // Repeat from the beginning.
                        .Repeat();

                var hangingConnectors =
                    ConnectorCreatedStream.SelectMany(
                        connector =>
                            nodeDeletedStream
                                .Where(
                                    node =>
                                        connector.Start.Node.Equals(node) || connector.End.Node.Equals(node))
                                .Select(_ => connector));

                ConnectorDeletedStream = connectorDeletedStream.Merge(hangingConnectors); //disconnects trigger deletion
            }
        }

        public class GraphModelWithDisconnect<TNode, TMetaData> : GraphModel<TNode, TMetaData>
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

            var workspace =
                new GraphModelWithDisconnect<string, int>(
                    disconnect, startConnection, endConnection, deleteNode);

            workspace.ConnectorCreatedStream.Dump("  ConnectorCreated");
            workspace.BeginNewConnectionStream.Dump("  ConnectionStarted");
            workspace.ConnectorDeletedStream.Dump("  ConnectorDeleted");

            workspace.ConnectorCreatedStream.Take(2).Subscribe(disconnect.OnNext);
            workspace.ConnectorDeletedStream
                .Select(_ => new Connection<string, int> { Node = nodes[2] } as Connection<string, int>?)
                .Subscribe(endConnection.OnNext);

            startConnection.OnNext(new Connection<string, int> { Node = nodes[0] });
            startConnection.OnNext(new Connection<string, int> { Node = nodes[1] });
            endConnection.OnNext(new Connection<string, int> { Node = nodes[1] });

            Assert.Pass("Success");
        }
    }
}
