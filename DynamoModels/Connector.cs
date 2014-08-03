namespace Dynamo.UI.Models
{
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
}