﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Util;
using GRPExplorerLib.BigFile.Versions;
using GRPExplorerLib.BigFile.Files.Archetypes;

namespace GRPExplorerLib.BigFile
{
    public class BigFileFile
    {
        private string name;
        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(name) ?
                    "NULL_FILE_NAME" :
                    name;
            }
        }
        public IBigFileFileInfo FileInfo { get; set; }
        public BigFileFolder ParentFolder { get; }
        private BigFileFile[] fileReferences;
        public BigFileFile[] FileReferences
        {
            get { return fileReferences; }
            set
            {
                fileReferences = value;
            }
        }
        public FileMappingData MappingData { get; set; }
        public BigFileFileArchetype Archetype { get; private set; }

        public string FullFolderPath
        {
            get
            {
                BigFileFolder folder = ParentFolder;
                string fullName = "";
                while (folder != null && folder.ParentFolder != null)
                {
                    fullName = folder.Name + "/" + fullName;
                    folder = folder.ParentFolder;
                }
                fullName += "/";
                return fullName;
            }
        }

        public BigFileFile(IBigFileFileInfo _fileInfo, BigFileFolder _parentFolder)
        {
            FileInfo = _fileInfo;
            ParentFolder = _parentFolder;
            name = FileInfo.Name.EncodeToGoodString();
            Archetype = this.CreateArchetype();
            Archetype.File = this;
        }

        public bool Is<T>() where T : BigFileFileArchetype
        {
            return Archetype is T;
        }

        public T ArchetypeAs<T>() where T : BigFileFileArchetype
        {
            if (Is<T>())
                return Archetype as T;
            return null;
        }

        public void Load(byte[] buffer, int size)
        {
            Archetype.Load(buffer, size, fileReferences);
        }

        public void Unload()
        {
            Archetype.Unload();
        }
    }
}
