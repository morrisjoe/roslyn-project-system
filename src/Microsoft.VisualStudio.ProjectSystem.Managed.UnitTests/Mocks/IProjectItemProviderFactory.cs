﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Moq;

namespace Microsoft.VisualStudio.Mocks
{
    internal class IProjectItemProviderFactory
    {
        public static IProjectItemProvider Create()
        {
            return Mock.Of<IProjectItemProvider>();
        }

        public static IProjectItemProvider CreateWithAdd(IProjectTree inputTree)
        {
            var mock = new Mock<IProjectItemProvider>();

            mock.Setup(a => a.AddAsync(It.IsAny<string>()))
                .Returns<string>( path =>
                {
                    var fileName = Path.GetFileName(path);
                    var parentFolder = Path.GetDirectoryName(path);
                    var newSubTree = ProjectTreeParser.Parse($@"{fileName}, FilePath: ""{path}""");

                    // Find the node that has the parent folder and add the new node as a child.
                    foreach (var node in inputTree.GetSelfAndDescendentsBreadthFirst())
                    {
                        if (node.FilePath.TrimEnd(Path.DirectorySeparatorChar).Equals(parentFolder))
                        {
                            IProjectTree child;
                            if (node.TryFindImmediateChild(fileName, out child) && !child.Flags.IsIncludedInProject())
                            {
                                var newFlags = child.Flags.Remove(ProjectTreeFlags.Common.IncludeInProjectCandidate);
                                child.SetProperties(flags: newFlags);
                            }
                            else
                            {
                                node.Add(newSubTree);
                            }
                            return Task.FromResult(Mock.Of<IProjectItem>());
                        }
                    }

                    return Task.FromResult<IProjectItem>(null);
                });

            return mock.Object;
        }
    }
}
