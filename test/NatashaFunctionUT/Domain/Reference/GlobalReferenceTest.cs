﻿using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using Xunit;

namespace NatashaFunctionUT.Reference
{
    [Trait("基础功能测试", "Reference")]
    public class GlobalReferenceTest : ReferencePrepare
    {
        [Fact(DisplayName = "默认引用数量")]
        public void GlobalReference()
        {
            IEnumerable<string> paths = DependencyContext
                .Default
                .CompileLibraries.SelectMany(cl => cl.ResolveReferencePaths());


            var count = NatashaDomain.DefaultDomain._referenceCache.Count;
            Assert.True(DefaultUsing.HasElement("System.Threading"));
            Assert.False(DefaultUsing.HasElement("System.IO"));
            Assert.True(paths.Count() <= NatashaDomain._excludeCount + count);
        }
        [Fact(DisplayName = "[默认引用]排重测试")]
        public void DefaultDistinctReference()
        {
            var domain = DomainManagement.Random();
            var assembilies = AssemblyLoadContext.Default.Assemblies;
            int count = 0;
            foreach (var item in assembilies)
            {
                if (!item.IsDynamic && item.Location != string.Empty)
                {
                    domain._referenceCache.AddReference(item);
                    count += 1;
                }
                if (count == 10)
                {
                    break;
                }
            }
            //Assembly Path='C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.12\System.Private.CoreLib.dll'
            var references = domain._referenceCache.CombineReferences(NatashaDomain.DefaultDomain._referenceCache, LoadBehaviorEnum.UseBeforeIfExist);
            Assert.Equal(NatashaDomain.DefaultDomain._referenceCache.Count, references.Count());
        }

        [Fact(DisplayName = "[合并版本引用]排重测试")]
        public void DistinctReferenceWithoutCompare()
        {
            var sets = GetPortableExecutableReferences(LoadBehaviorEnum.None);
            //dapper + json + plugin 
            Assert.Equal(3, sets.Count);

        }

        [Fact(DisplayName = "[高版本引用]排重测试")]
        public void DistinctReferenceWithHighVersion()
        {
            var sets = GetPortableExecutableReferences(LoadBehaviorEnum.UseHighVersion);
            //dapper + json + plugin 
            Assert.Equal(3, sets.Count);
        }

        [Fact(DisplayName = "[低版本引用]排重测试")]
        public void DistinctReferenceWithLowVersion()
        {
            var sets = GetPortableExecutableReferences(LoadBehaviorEnum.UseLowVersion);
            //dapper + plugin
            Assert.Equal(2, sets.Count);
        }

        [Fact(DisplayName = "[非同名引用]排重测试")]
        public void DistinctReferenceWithExistVersion()
        {
            var sets = GetPortableExecutableReferences(LoadBehaviorEnum.UseBeforeIfExist);
            //dapper + plugin
            Assert.Equal(2, sets.Count);
        }
    } 
}