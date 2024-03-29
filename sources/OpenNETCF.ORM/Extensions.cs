﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace OpenNETCF.ORM
{
    public static class Extensions
    {
        internal static bool IsNullable(this Type type)
        {
            if (!type.IsGenericType) return false;
            if (type.Name == "Nullable`1") return true;

            return false;
        }

        public static Type ToManagedType(this DbType type)
        {
            return type.ToManagedType(false);
        }

        public static Type ToManagedType(this DbType type, bool isNullable)
        {
            switch (type)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return typeof(string);
                case DbType.Boolean:
                    return isNullable ? typeof(bool?) : typeof(bool);
                case DbType.Int16:
                    return isNullable ? typeof(short?) : typeof(short);
                case DbType.UInt16:
                    return isNullable ? typeof(ushort?) : typeof(ushort);
                case DbType.Int32:
                    return isNullable ? typeof(int?) : typeof(int);
                case DbType.UInt32:
                    return isNullable ? typeof(uint?) : typeof(uint);
                case DbType.DateTime:
                    return isNullable ? typeof(DateTime?) : typeof(DateTime);
                case DbType.Decimal:
                    return isNullable ? typeof(decimal?) : typeof(decimal);
                case DbType.Currency:
                    return isNullable ? typeof(decimal?) : typeof(decimal);
                case DbType.Double:
                    return isNullable ? typeof(double?) : typeof(double);
                case DbType.Int64:
                    return isNullable ? typeof(long?) : typeof(long);
                case DbType.UInt64:
                    return isNullable ? typeof(ulong?) : typeof(ulong);
                case DbType.Byte:
                    return isNullable ? typeof(byte?) : typeof(byte);
                case DbType.Guid:
                    return isNullable ? typeof(Guid?) : typeof(Guid);
                case DbType.Binary:
                    return typeof(byte[]);
                default:
                    throw new NotSupportedException();
            }
        }

        public static DbType ToDbType(this Type type)
        {
            string typeName = type.FullName;

            if (type.IsNullable())
            {
                typeName = type.GetGenericArguments()[0].FullName;
            }

            switch (typeName)
            {
                case "System.String":
                    return DbType.String;
                case "System.Boolean":
                    return DbType.Boolean;
                case "System.Int16":
                    return DbType.Int16;
                case "System.UInt16":
                    return DbType.UInt16;
                case "System.Int32":
                    return DbType.Int32;
                case "System.UInt32":
                    return DbType.UInt32;
                case "System.DateTime":
                    return DbType.DateTime;
                case "System.TimeSpan":
                    return DbType.Int64;

                case "System.Decimal":
                    return DbType.Decimal;
                case "System.Double":
                    return DbType.Double;
                case "System.Int64":
                    return DbType.Int64;
                case "System.UInt64":
                    return DbType.UInt64;
                case "System.Byte":
                    return DbType.Byte;
                case "System.Char":
                    return DbType.Byte;
                case "System.Guid":
                    return DbType.Guid;

                case "System.Byte[]":
                    return DbType.Binary;

                default:
                    if (type.IsEnum)
                    {
                        return DbType.Int32;
                    }

                    // everything else is an "object" and requires a custom serializer/deserializer
                    return DbType.Object;
            }
        }

        public static DbType ParseToDbType(this string dbTypeName)
        {
            switch (dbTypeName.ToLower())
            {
                case "datetime":
                    return DbType.DateTime;
                case "bigint":
                    return DbType.Int64;
                case "int":
                    return DbType.Int32;
                case "smallint":
                    return DbType.Int16;
                case "nvarchar":
                    return DbType.String;
                case "varchar":
                    return DbType.AnsiString;
                case "nchar":
                    return DbType.StringFixedLength;
                case "char":
                    return DbType.AnsiStringFixedLength;
                case "bit":
                    return DbType.Boolean;
                case "image":
                    return DbType.Object;
                case "tinyint":
                    return DbType.Byte;
                case "numeric":
                    return DbType.Decimal;
                case "float":
                    return DbType.Double;
                case "uniqueidentifier":
                    return DbType.Guid;
                case "ntext":
                    return DbType.Binary; // TODO: verify this
                default:
                    throw new NotSupportedException(
                        string.Format("Unable to determine convert string '{0}' to DbType", dbTypeName));
            }
        }

        public static string ToSqlTypeString(this DbType type)
        {
            switch (type)
            {
                case DbType.DateTime:
                    return "datetime";
                case DbType.Time:
                case DbType.Int64:
                case DbType.UInt64:
                    return "bigint";
                case DbType.Int32:
                case DbType.UInt32:
                    return "integer";
                case DbType.Int16:
                case DbType.UInt16:
                    return "smallint";
                case DbType.String:
                    return "nvarchar";
                case DbType.StringFixedLength:
                    return "nchar";
                case DbType.AnsiString:
                    return "varchar";
                case DbType.AnsiStringFixedLength:
                    return "char";
                case DbType.Boolean:
                    return "bit";
                case DbType.Object:
                    return "image";
                case DbType.Byte:
                    return "tinyint";
                case DbType.Decimal:
                    return "numeric";
                case DbType.Currency:
                    return "numeric";
                case DbType.Double:
                    return "float";
                case DbType.Guid:
                    return "uniqueidentifier";

                default:
                    throw new NotSupportedException(
                        string.Format("Unable to determine convert DbType '{0}' to string", type.ToString()));
            }
        }

        public static bool UnderlyingTypeIs<T>(this Type checkType)
        {
            if ((checkType.IsGenericType) && (checkType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))))
            {
                return Nullable.GetUnderlyingType(checkType).Equals(typeof(T));
            }
            else
            {
                return checkType.Equals(typeof(T));
            }
        }

        /// <summary>
        /// The purpose of this function is to ensure that the name given to the tables match the constraints of
        /// all the compatible databases. This includes Oracle which has a 30 character length limit.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool MatchTableNamingConstraints(string tableName)
        {
            if (tableName != null && tableName.Length <= 30) return true;
            return false;
        }

        /// <summary>
        /// The purpose of this function is to ensure that the name given to the fields match the constraints of
        /// all the compatible databases. This includes Oracle which has a 30 character length limit.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool MatchFieldNamingConstraints(string fieldName)
        {
            if (fieldName != null && fieldName.Length <= 30) return true;
            return false;
        }
    }
}
