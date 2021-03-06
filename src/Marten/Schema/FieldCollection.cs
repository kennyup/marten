﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;

namespace Marten.Schema
{
    public class FieldCollection
    {
        private readonly string _dataLocator;
        private readonly Type _documentType;
        private readonly StoreOptions _options;
        private readonly ConcurrentDictionary<string, IField> _fields = new ConcurrentDictionary<string, IField>();

        public FieldCollection(string dataLocator, Type documentType, StoreOptions options)
        {
            _dataLocator = dataLocator;
            _documentType = documentType;
            _options = options;
        }

        protected void setField(string name, IField field)
        {
            _fields[name] = field;
        }

        protected IEnumerable<IField> fields()
        {
            return _fields.Values;
        }

        public IField FieldFor(IEnumerable<MemberInfo> members)
        {
            if (members.Count() == 1)
            {
                return FieldFor(members.Single());
            }

            var key = members.Select(x => x.Name).Join("");
            return _fields.GetOrAdd(key,
                _ => new JsonLocatorField(_dataLocator, _options.Serializer().EnumStorage, members.ToArray()));
        }

        public IField FieldFor(MemberInfo member)
        {
            return _fields.GetOrAdd(member.Name,
                name => new JsonLocatorField(_dataLocator, _options, _options.Serializer().EnumStorage, member));
        }

        public IField FieldFor(string memberName)
        {
            return _fields.GetOrAdd(memberName, name =>
            {
                var member = _documentType.GetProperties().FirstOrDefault(x => x.Name == name).As<MemberInfo>() ??
                             _documentType.GetFields().FirstOrDefault(x => x.Name == name);

                if (member == null) return null;

                return new JsonLocatorField(_dataLocator, _options, _options.Serializer().EnumStorage, member);
            });
        }

        public IField FieldForColumn(string columnName)
        {
            return _fields.Values.FirstOrDefault(x => x.ColumnName == columnName);
        }
    }
}