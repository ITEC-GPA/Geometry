using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

/// <summary>
/// Round trip of the tests through a DataContractSerializer, in place of BinaryFormatter: removed from .NET 9 and unsafe with data from
/// others (architecture review R10). The types without a known data contract are written with their CLR name, as BinaryFormatter did:
/// acceptable only because the tests read back their own data. A <see cref="Type"/> is written by name, as BinaryFormatter did.
/// Same shape as the formatter it replaces; each value is written with its type and length, so several values can share a stream.
/// In the global namespace so that every test namespace sees it.
/// </summary>
internal sealed class ContractFormatter
{
    private const string TypeValue = "System.Type:";

    public void Serialize(Stream stream, object value)
    {
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
        {
            if (value is Type type) { writer.Write(TypeValue + type.AssemblyQualifiedName); return; }
            using (var buffer = new MemoryStream())
            {
                Serializer(value.GetType()).WriteObject(buffer, value);
                writer.Write(value.GetType().AssemblyQualifiedName); writer.Write((int)buffer.Length); writer.Write(buffer.ToArray());
            }
        }
    }

    public object Deserialize(Stream stream)
    {
        using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
        {
            string name = reader.ReadString();
            if (name.StartsWith(TypeValue, StringComparison.Ordinal)) return Type.GetType(name.Substring(TypeValue.Length), true);
            var type = Type.GetType(name, true);
            using (var buffer = new MemoryStream(reader.ReadBytes(reader.ReadInt32())))
                return Serializer(type).ReadObject(buffer);
        }
    }

    private static DataContractSerializer Serializer(Type type) => new DataContractSerializer(type, new DataContractSerializerSettings
    {
        DataContractResolver = new ClrNames(), PreserveObjectReferences = true, MaxItemsInObjectGraph = int.MaxValue
    });

    private sealed class ClrNames : DataContractResolver
    {
        private const string Namespace = "urn:test:clr";
        public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver,
            out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            if (knownTypeResolver.TryResolveType(type, declaredType, null, out typeName, out typeNamespace)) return true;
            var names = new XmlDictionary();
            typeName = names.Add(XmlConvert.EncodeLocalName(type.AssemblyQualifiedName)); typeNamespace = names.Add(Namespace);
            return true;
        }
        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
            => typeNamespace == Namespace ? Type.GetType(XmlConvert.DecodeName(typeName), true)
                : knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);
    }
}
