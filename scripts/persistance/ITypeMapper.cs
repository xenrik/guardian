using System;

public interface ITypeMapper {
    public string TypeToId(IPersistable value);

    public Type IdToType(string typeId);

    public class Delegate : ITypeMapper {
        private Func<IPersistable, string> typeMapper;
        private Func<string, Type> idMapper;

        public Delegate(
                Func<IPersistable, string> typeMapper,
                Func<string, Type> idMapper) {
            this.typeMapper = typeMapper;
            this.idMapper = idMapper;
        }

        public string TypeToId(IPersistable value) => typeMapper(value);
        public Type IdToType(string typeId) => idMapper(typeId);
    }
}