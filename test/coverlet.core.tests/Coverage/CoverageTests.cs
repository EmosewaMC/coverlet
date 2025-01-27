﻿// Copyright (c) Toni Solarin-Sodara
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Coverlet.Core.Abstractions;
using Coverlet.Core.Helpers;
using Coverlet.Core.Symbols;
using Moq;
using Xunit;

namespace Coverlet.Core.Tests
{
    public partial class CoverageTests
    {
        private readonly Mock<ILogger> _mockLogger = new();

        [Fact]
        public void TestCoverage()
        {
            string module = GetType().Assembly.Location;
            string pdb = Path.Combine(Path.GetDirectoryName(module), Path.GetFileNameWithoutExtension(module) + ".pdb");

            DirectoryInfo directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            File.Copy(module, Path.Combine(directory.FullName, Path.GetFileName(module)), true);
            File.Copy(pdb, Path.Combine(directory.FullName, Path.GetFileName(pdb)), true);

            // TODO: Find a way to mimick hits
            var instrumentationHelper =
                new InstrumentationHelper(new ProcessExitHandler(), new RetryHelper(), new FileSystem(), new Mock<ILogger>().Object,
                                          new SourceRootTranslator(module, new Mock<ILogger>().Object, new FileSystem(), new AssemblyAdapter()));

            var parameters = new CoverageParameters
            {
                IncludeFilters = new string[] { "[coverlet.tests.projectsample.excludedbyattribute*]*" },
                IncludeDirectories = Array.Empty<string>(),
                ExcludeFilters = Array.Empty<string>(),
                ExcludedSourceFiles = Array.Empty<string>(),
                ExcludeAttributes = Array.Empty<string>(),
                IncludeTestAssembly = false,
                SingleHit = false,
                MergeWith = string.Empty,
                UseSourceLink = false
            };

            var coverage = new Coverage(Path.Combine(directory.FullName, Path.GetFileName(module)), parameters, _mockLogger.Object, instrumentationHelper, new FileSystem(), new SourceRootTranslator(_mockLogger.Object, new FileSystem()), new CecilSymbolHelper());
            coverage.PrepareModules();

            CoverageResult result = coverage.GetCoverageResult();

            Assert.Empty(result.Modules);

            directory.Delete(true);
        }

        [Fact]
        public void TestCoverageWithTestAssembly()
        {
            string module = GetType().Assembly.Location;
            string pdb = Path.Combine(Path.GetDirectoryName(module), Path.GetFileNameWithoutExtension(module) + ".pdb");

            DirectoryInfo directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            File.Copy(module, Path.Combine(directory.FullName, Path.GetFileName(module)), true);
            File.Copy(pdb, Path.Combine(directory.FullName, Path.GetFileName(pdb)), true);

            var instrumentationHelper =
                new InstrumentationHelper(new ProcessExitHandler(), new RetryHelper(), new FileSystem(), new Mock<ILogger>().Object,
                                          new SourceRootTranslator(module, new Mock<ILogger>().Object, new FileSystem(), new AssemblyAdapter()));

            var parameters = new CoverageParameters
            {
                IncludeFilters = Array.Empty<string>(),
                IncludeDirectories = Array.Empty<string>(),
                ExcludeFilters = Array.Empty<string>(),
                ExcludedSourceFiles = Array.Empty<string>(),
                ExcludeAttributes = Array.Empty<string>(),
                IncludeTestAssembly = true,
                SingleHit = false,
                MergeWith = string.Empty,
                UseSourceLink = false
            };

            var coverage = new Coverage(Path.Combine(directory.FullName, Path.GetFileName(module)), parameters, _mockLogger.Object, instrumentationHelper, new FileSystem(),
                                        new SourceRootTranslator(module, _mockLogger.Object, new FileSystem(), new AssemblyAdapter()), new CecilSymbolHelper());
            coverage.PrepareModules();

            CoverageResult result = coverage.GetCoverageResult();

            Assert.NotEmpty(result.Modules);

            directory.Delete(true);
        }
    }
}
