using System;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Scramjet.CrmPlugins {
    public struct JsonEntityReference {
        public JsonEntityReference(string name, Guid guid) {
            this.entityname = name;
            this.entityguid = guid;
        }
        public string entityname { get; set; }
        public Guid entityguid { get; set; }
    }
}