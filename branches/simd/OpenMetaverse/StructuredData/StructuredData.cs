/*
 * Copyright (c) 2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Mono.Simd;
using Mono.Simd.Math;

namespace OpenMetaverse.StructuredData
{
    /// <summary>
    /// 
    /// </summary>
    public enum OSDType
    {
        /// <summary></summary>
        Unknown,
        /// <summary></summary>
        Boolean,
        /// <summary></summary>
        Integer,
        /// <summary></summary>
        Real,
        /// <summary></summary>
        String,
        /// <summary></summary>
        Guid,
        /// <summary></summary>
        Date,
        /// <summary></summary>
        URI,
        /// <summary></summary>
        Binary,
        /// <summary></summary>
        Map,
        /// <summary></summary>
        Array
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDException : Exception
    {
        public OSDException(string message) : base(message) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class OSD
    {
        public virtual OSDType Type { get { return OSDType.Unknown; } }

        public virtual bool AsBoolean() { return false; }
        public virtual int AsInteger() { return 0; }
        public virtual double AsReal() { return 0d; }
        public virtual string AsString() { return String.Empty; }
        public virtual Guid AsGuid() { return Guid.Empty; }
        public virtual DateTime AsDate() { return Utils.Epoch; }
        public virtual Uri AsUri() { return new Uri(String.Empty); }
        public virtual byte[] AsBinary() { return new byte[0]; }

        public override string ToString() { return "undef"; }

        public static OSD FromBoolean(bool value) { return new OSDBoolean(value); }
        public static OSD FromInteger(int value) { return new OSDInteger(value); }
        public static OSD FromInteger(uint value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(short value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(ushort value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(sbyte value) { return new OSDInteger((int)value); }
        public static OSD FromInteger(byte value) { return new OSDInteger((int)value); }
        public static OSD FromUInteger(uint value) { return new OSDBinary(value); }
        public static OSD FromReal(double value) { return new OSDReal(value); }
        public static OSD FromReal(float value) { return new OSDReal((double)value); }
        public static OSD FromString(string value) { return new OSDString(value); }
        public static OSD FromGuid(Guid value) { return new OSDGuid(value); }
        public static OSD FromDate(DateTime value) { return new OSDDate(value); }
        public static OSD FromUri(Uri value) { return new OSDURI(value); }
        public static OSD FromBinary(byte[] value) { return new OSDBinary(value); }
        public static OSD FromBinary(long value) { return new OSDBinary(value); }
        public static OSD FromBinary(ulong value) { return new OSDBinary(value); }

        public static OSD FromVector2(Vector2 value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            return array;
        }

        public static OSD FromVector3(Vector3f value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            return array;
        }

        public static OSD FromVector3d(Vector3d value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            return array;
        }

        public static OSD FromVector4(Vector4f value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            array.Add(OSD.FromReal(value.W));
            return array;
        }

        public static OSD FromQuaternion(Quaternionf value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.X));
            array.Add(OSD.FromReal(value.Y));
            array.Add(OSD.FromReal(value.Z));
            array.Add(OSD.FromReal(value.W));
            return array;
        }

        public static OSD FromColor4(Color4 value)
        {
            OSDArray array = new OSDArray();
            array.Add(OSD.FromReal(value.R));
            array.Add(OSD.FromReal(value.G));
            array.Add(OSD.FromReal(value.B));
            array.Add(OSD.FromReal(value.A));
            return array;
        }

        public static OSD FromObject(object value)
        {
            if (value == null) { return new OSD(); }
            else if (value is bool) { return new OSDBoolean((bool)value); }
            else if (value is int) { return new OSDInteger((int)value); }
            else if (value is uint) { return new OSDBinary((uint)value); }
            else if (value is short) { return new OSDInteger((int)(short)value); }
            else if (value is ushort) { return new OSDInteger((int)(ushort)value); }
            else if (value is sbyte) { return new OSDInteger((int)(sbyte)value); }
            else if (value is byte) { return new OSDInteger((int)(byte)value); }
            else if (value is double) { return new OSDReal((double)value); }
            else if (value is float) { return new OSDReal((double)(float)value); }
            else if (value is string) { return new OSDString((string)value); }
            else if (value is Guid) { return new OSDGuid((Guid)value); }
            else if (value is DateTime) { return new OSDDate((DateTime)value); }
            else if (value is Uri) { return new OSDURI((Uri)value); }
            else if (value is byte[]) { return new OSDBinary((byte[])value); }
            else if (value is long) { return new OSDBinary((long)value); }
            else if (value is ulong) { return new OSDBinary((ulong)value); }
            else if (value is Vector2) { return FromVector2((Vector2)value); }
            else if (value is Vector3f) { return FromVector3((Vector3f)value); }
            else if (value is Vector3d) { return FromVector3d((Vector3d)value); }
            else if (value is Vector4f) { return FromVector4((Vector4f)value); }
            else if (value is Quaternionf) { return FromQuaternion((Quaternionf)value); }
            else if (value is Color4) { return FromColor4((Color4)value); }
            else return new OSD();
        }

        public static object ToObject(Type type, OSD value)
        {
            if (type == typeof(ulong))
            {
                if (value.Type == OSDType.Binary)
                {
                    byte[] bytes = value.AsBinary();
                    return Utils.BytesToUInt64(bytes);
                }
                else
                {
                    return (ulong)value.AsInteger();
                }
            }
            else if (type == typeof(uint))
            {
                if (value.Type == OSDType.Binary)
                {
                    byte[] bytes = value.AsBinary();
                    return Utils.BytesToUInt(bytes);
                }
                else
                {
                    return (uint)value.AsInteger();
                }
            }
            else if (type == typeof(ushort))
            {
                return (ushort)value.AsInteger();
            }
            else if (type == typeof(byte))
            {
                return (byte)value.AsInteger();
            }
            else if (type == typeof(short))
            {
                return (short)value.AsInteger();
            }
            else if (type == typeof(string))
            {
                return value.AsString();
            }
            else if (type == typeof(bool))
            {
                return value.AsBoolean();
            }
            else if (type == typeof(float))
            {
                return (float)value.AsReal();
            }
            else if (type == typeof(double))
            {
                return value.AsReal();
            }
            else if (type == typeof(int))
            {
                return value.AsInteger();
            }
            else if (type == typeof(Guid))
            {
                return value.AsGuid();
            }
            else if (type == typeof(Vector3f))
            {
                if (value.Type == OSDType.Array)
                    return ((OSDArray)value).AsVector3();
                else
                    return Vector3f.Zero;
            }
            else if (type == typeof(Vector4f))
            {
                if (value.Type == OSDType.Array)
                    return ((OSDArray)value).AsVector4();
                else
                    return Vector4f.Zero;
            }
            else if (type == typeof(Quaternionf))
            {
                if (value.Type == OSDType.Array)
                    return ((OSDArray)value).AsQuaternion();
                else
                    return Quaternionf.Identity;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Uses reflection to create an SDMap from all of the SD
        /// serializable types in an object
        /// </summary>
        /// <param name="obj">Class or struct containing serializable types</param>
        /// <returns>An SDMap holding the serialized values from the
        /// container object</returns>
        public static OSDMap SerializeMembers(object obj)
        {
            Type t = obj.GetType();
            FieldInfo[] fields = t.GetFields();

            OSDMap map = new OSDMap(fields.Length);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!Attribute.IsDefined(field, typeof(NonSerializedAttribute)))
                {
                    OSD serializedField = OSD.FromObject(field.GetValue(obj));

                    if (serializedField.Type != OSDType.Unknown || field.FieldType == typeof(string) || field.FieldType == typeof(byte[]))
                        map.Add(field.Name, serializedField);
                }
            }

            return map;
        }

        /// <summary>
        /// Uses reflection to deserialize member variables in an object from
        /// an SDMap
        /// </summary>
        /// <param name="obj">Reference to an object to fill with deserialized
        /// values</param>
        /// <param name="serialized">Serialized values to put in the target
        /// object</param>
        public static void DeserializeMembers(ref object obj, OSDMap serialized)
        {
            Type t = obj.GetType();
            FieldInfo[] fields = t.GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (!Attribute.IsDefined(field, typeof(NonSerializedAttribute)))
                {
                    OSD serializedField;
                    if (serialized.TryGetValue(field.Name, out serializedField))
                        field.SetValue(obj, ToObject(field.FieldType, serializedField));
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDBoolean : OSD
    {
        private bool value;

        private static byte[] trueBinary = { 0x31 };
        private static byte[] falseBinary = { 0x30 };

        public override OSDType Type { get { return OSDType.Boolean; } }

        public OSDBoolean(bool value)
        {
            this.value = value;
        }

        public override bool AsBoolean() { return value; }
        public override int AsInteger() { return value ? 1 : 0; }
        public override double AsReal() { return value ? 1d : 0d; }
        public override string AsString() { return value ? "1" : "0"; }
        public override byte[] AsBinary() { return value ? trueBinary : falseBinary; }

        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDInteger : OSD
    {
        private int value;

        public override OSDType Type { get { return OSDType.Integer; } }

        public OSDInteger(int value)
        {
            this.value = value;
        }

        public override bool AsBoolean() { return value != 0; }
        public override int AsInteger() { return value; }
        public override double AsReal() { return (double)value; }
        public override string AsString() { return value.ToString(); }
        public override byte[] AsBinary() { return Utils.IntToBytes(value); }
        
        public override string ToString() { return AsString(); }        
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class OSDReal : OSD
    {
        private double value;

        public override OSDType Type { get { return OSDType.Real; } }

        public OSDReal(double value)
        {
            this.value = value;
        }

        public override bool AsBoolean() { return (!Double.IsNaN(value) && value != 0d); }
        public override int AsInteger() { 
            if ( Double.IsNaN( value ) )
                return 0;
            if ( value > (double)Int32.MaxValue )
                return Int32.MaxValue;
            if ( value < (double)Int32.MinValue )
                return Int32.MinValue;
            return (int)Math.Round( value );
        }
 
        public override double AsReal() { return value; }
        public override string AsString() { return value.ToString(Helpers.EnUsCulture); }
        public override byte[] AsBinary() { return Utils.DoubleToBytes(value); }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDString : OSD
    {
        private string value;

        public override OSDType Type { get { return OSDType.String; } }

        public OSDString(string value)
        {
            // Refuse to hold null pointers
            if (value != null)
                this.value = value;
            else
                this.value = String.Empty;
        }

        public override bool AsBoolean()
        {
            if (String.IsNullOrEmpty(value))
                return false;

            if (value == "0" || value.ToLower() == "false")
                return false;

            return true;
        }

        public override int AsInteger()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return (int)Math.Floor( dbl );
            else
                return 0;
        }
        public override double AsReal()
        {
            double dbl;
            if (Double.TryParse(value, out dbl))
                return dbl;
            else
                return 0d;
        }
        public override string AsString() { return value; } 
        public override byte[] AsBinary() { return Encoding.UTF8.GetBytes( value ); }
        public override Guid AsGuid()
        {
            Guid Guid;
            if (GuidExtensions.TryParse(value, out Guid))
                return Guid;
            else
                return Guid.Empty;
        }
        public override DateTime AsDate()
        {
            DateTime dt;
            if (DateTime.TryParse(value, out dt))
                return dt;
            else
                return Utils.Epoch;
        }
        public override Uri AsUri() { return new Uri(value); }

        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDGuid : OSD
    {
        private Guid value;

        public override OSDType Type { get { return OSDType.Guid; } }

        public OSDGuid(Guid value)
        {
            this.value = value;
        }

        public override bool AsBoolean() { return (value == Guid.Empty) ? false : true; }
        public override string AsString() { return value.ToString(); }
        public override Guid AsGuid() { return value; }
        public override byte[] AsBinary() { return value.GetBytes(); }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDDate : OSD
    {
        private DateTime value;

        public override OSDType Type { get { return OSDType.Date; } }

        public OSDDate(DateTime value)
        {
            this.value = value;
        }

        public override string AsString() 
        { 
            string format;
            if ( value.Millisecond > 0 )
                format = "yyyy-MM-ddTHH:mm:ss.ffZ";
            else
                format = "yyyy-MM-ddTHH:mm:ssZ";
            return value.ToUniversalTime().ToString( format );        
        }
        
         public override byte[] AsBinary()
         {
            TimeSpan ts = value.ToUniversalTime() - new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
            return Utils.DoubleToBytes(ts.TotalSeconds);
        }

        public override DateTime AsDate() { return value; }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDURI : OSD
    {
        private Uri value;

        public override OSDType Type { get { return OSDType.URI; } }

        public OSDURI(Uri value)
        {
            this.value = value;
        }

        public override string AsString() { return value.ToString(); }
        public override Uri AsUri() { return value; }
        public override byte[] AsBinary() { return Encoding.UTF8.GetBytes(value.ToString()); }
        public override string ToString() { return AsString(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDBinary : OSD
    {
        private byte[] value;

        public override OSDType Type { get { return OSDType.Binary; } }

        public OSDBinary(byte[] value)
        {
            if (value != null)
                this.value = value;
            else
                this.value = new byte[0];
        }

        public OSDBinary(uint value)
        {
            this.value = Utils.UIntToBytes(value);
        }

        public OSDBinary(long value)
        {
            this.value = Utils.Int64ToBytes(value);
        }

        public OSDBinary(ulong value)
        {
            this.value = Utils.UInt64ToBytes(value);
        }

        public override string AsString() { return Convert.ToBase64String(value); }
        public override byte[] AsBinary() { return value; }

        public override string ToString()
        {
            return Utils.BytesToHexString(value, null);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDMap : OSD, IDictionary<string, OSD>
    {
        private Dictionary<string, OSD> value;

        public override OSDType Type { get { return OSDType.Map; } }

        public OSDMap()
        {
            value = new Dictionary<string, OSD>();
        }

        public OSDMap(int capacity)
        {
            value = new Dictionary<string, OSD>(capacity);
        }

        public OSDMap(Dictionary<string, OSD> value)
        {
            if (value != null)
                this.value = value;
            else
                this.value = new Dictionary<string, OSD>();
        }

        public override bool AsBoolean() { return value.Count > 0; }

        public override string ToString()
        {
            return OSDParser.SerializeLLSDNotationFormatted(this);
        }

        #region IDictionary Implementation

        public int Count { get { return value.Count; } }
        public bool IsReadOnly { get { return false; } }
        public ICollection<string> Keys { get { return value.Keys; } }
        public ICollection<OSD> Values { get { return value.Values; } }
        public OSD this[string key]
        {
            get
            {
                OSD llsd;
                if (this.value.TryGetValue(key, out llsd))
                    return llsd;
                else
                    return new OSD();
            }
            set { this.value[key] = value; }
        }

        public bool ContainsKey(string key)
        {
            return value.ContainsKey(key);
        }

        public void Add(string key, OSD llsd)
        {
            value.Add(key, llsd);
        }

        public void Add(KeyValuePair<string, OSD> kvp)
        {
            value.Add(kvp.Key, kvp.Value);
        }

        public bool Remove(string key)
        {
            return value.Remove(key);
        }

        public bool TryGetValue(string key, out OSD llsd)
        {
            return value.TryGetValue(key, out llsd);
        }

        public void Clear()
        {
            value.Clear();
        }

        public bool Contains(KeyValuePair<string, OSD> kvp)
        {
            // This is a bizarre function... we don't really implement it
            // properly, hopefully no one wants to use it
            return value.ContainsKey(kvp.Key);
        }

        public void CopyTo(KeyValuePair<string, OSD>[] array, int index)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, OSD> kvp)
        {
            return this.value.Remove(kvp.Key);
        }

        public System.Collections.IDictionaryEnumerator GetEnumerator()
        {
            return value.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, OSD>> IEnumerable<KeyValuePair<string, OSD>>.GetEnumerator()
        {
            return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return value.GetEnumerator();
        }

        #endregion IDictionary Implementation
    }

    /// <summary>
    /// 
    /// </summary>
    public class OSDArray : OSD, IList<OSD>
    {
        private List<OSD> value;

        public override OSDType Type { get { return OSDType.Array; } }

        public OSDArray()
        {
            value = new List<OSD>();
        }

        public OSDArray(int capacity)
        {
            value = new List<OSD>(capacity);
        }

        public OSDArray(List<OSD> value)
        {
            if (value != null)
                this.value = value;
            else
                this.value = new List<OSD>();
        }

        public Vector2 AsVector2()
        {
            Vector2 vector = Vector2.Zero;

            if (this.Count == 2)
            {
                vector.X = (float)this[0].AsReal();
                vector.Y = (float)this[1].AsReal();
            }

            return vector;
        }

        public Vector3f AsVector3()
        {
            Vector3f vector = Vector3f.Zero;

            if (this.Count == 3)
            {
                vector.X = (float)this[0].AsReal();
                vector.Y = (float)this[1].AsReal();
                vector.Z = (float)this[2].AsReal();
            }

            return vector;
        }

        public Vector3d AsVector3d()
        {
            Vector3d vector = Vector3d.Zero;

            if (this.Count == 3)
            {
                vector.X = this[0].AsReal();
                vector.Y = this[1].AsReal();
                vector.Z = this[2].AsReal();
            }

            return vector;
        }

        public Vector4f AsVector4()
        {
            Vector4f vector = Vector4f.Zero;

            if (this.Count == 4)
            {
                vector.X = (float)this[0].AsReal();
                vector.Y = (float)this[1].AsReal();
                vector.Z = (float)this[2].AsReal();
                vector.W = (float)this[3].AsReal();
            }

            return vector;
        }

        public Quaternionf AsQuaternion()
        {
            Quaternionf quaternion = Quaternionf.Identity;

            if (this.Count == 4)
            {
                quaternion.X = (float)this[0].AsReal();
                quaternion.Y = (float)this[1].AsReal();
                quaternion.Z = (float)this[2].AsReal();
                quaternion.W = (float)this[3].AsReal();
            }

            return quaternion;
        }

        public Color4 AsColor4()
        {
            Color4 color = Color4.Black;

            if (this.Count == 4)
            {
                color.R = (float)this[0].AsReal();
                color.G = (float)this[1].AsReal();
                color.B = (float)this[2].AsReal();
                color.A = (float)this[3].AsReal();
            }

            return color;
        }

        public override bool AsBoolean() { return value.Count > 0; }

        public override string ToString()
        {
            return OSDParser.SerializeLLSDNotationFormatted(this);
        }

        #region IList Implementation

        public int Count { get { return value.Count; } }
        public bool IsReadOnly { get { return false; } }
        public OSD this[int index]
        {
            get { return value[index]; }
            set { this.value[index] = value; }
        }

        public int IndexOf(OSD llsd)
        {
            return value.IndexOf(llsd);
        }

        public void Insert(int index, OSD llsd)
        {
            value.Insert(index, llsd);
        }

        public void RemoveAt(int index)
        {
            value.RemoveAt(index);
        }

        public void Add(OSD llsd)
        {
            value.Add(llsd);
        }

        public void Add(string str)
        {
            // This is so common that we throw a little helper in here
            value.Add(OSD.FromString(str));
        }

        public void Clear()
        {
            value.Clear();
        }

        public bool Contains(OSD llsd)
        {
            return value.Contains(llsd);
        }

        public bool Contains(string element)
        {
            for (int i = 0; i < value.Count; i++)
            {
                if (value[i].Type == OSDType.String && value[i].AsString() == element)
                    return true;
            }

            return false;
        }

        public void CopyTo(OSD[] array, int index)
        {
            throw new NotImplementedException();
        }

        public bool Remove(OSD llsd)
        {
            return value.Remove(llsd);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return value.GetEnumerator();
        }

        IEnumerator<OSD> IEnumerable<OSD>.GetEnumerator()
        {
            return value.GetEnumerator();
        }

        #endregion IList Implementation
    }
}
