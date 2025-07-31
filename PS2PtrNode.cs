using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using ReClassNET.Controls;
using ReClassNET.Extensions;
using ReClassNET.Memory;
using ReClassNET.Nodes;
using ReClassNET.UI;

namespace PCSX2Plugin
{
    public class EEMemNode : BaseClassWrapperNode
    {
        private readonly MemoryBuffer memory = new MemoryBuffer();

        public override int MemorySize => 0x4;

        protected override bool PerformCycleCheck => false;

        public override void GetUserInterfaceInfo(out string name, out Image icon)
        {
            name = "Get EEMem Address";
            icon = Properties.Resources.logo_pcsx2;
        }

        public override void Initialize()
        {
            var node = ClassNode.Create();
            node.Initialize();
            node.AddBytes(64);
            ChangeInnerNode(node);
        }

        public override Size Draw(DrawContext context, int x, int y)
        {
            if (IsHidden && !IsWrapped)
            {
                return DrawHidden(context, x, y);
            }
            Name = "int EEMem"; //  auto generate struct name to fix any padding issues

            var origX = x;
            var origY = y;

            AddSelection(context, x, y, context.Font.Height);

            x = AddOpenCloseIcon(context, x, y);
            x = AddIcon(context, x, y, context.IconProvider.Pointer, -1, HotSpotType.None);

            var tx = x;
            x = AddAddressOffset(context, x, y);

            /* RENDER ROW ITEM */
            long ptr = 0;
            PS2Helpers.GetEEMem(context, ref ptr);
            x = AddText(context, x, y, context.Settings.TypeColor, HotSpot.NoneId, "EEMem") + context.Font.Width;
            x = AddText(context, x, y, Color.Red, HotSpot.NoneId, " -> ");
            x = AddText(context, x, y, context.Settings.ValueColor, HotSpot.AddressId, String.Format("0x{0:X}", ptr));
            x += context.Font.Width;

            AddComment(context, x, y);
            DrawInvalidMemoryIndicatorIcon(context, y);
            AddContextDropDownIcon(context, y);
            AddDeleteIcon(context, y);
            y += context.Font.Height;

            var size = new Size(x - origX, y - origY);

            if (LevelsOpen[context.Level])
            {
                IntPtr addr = (IntPtr)(ptr + Offset);
                memory.Size = InnerNode.MemorySize;
                memory.UpdateFrom(context.Process, addr);

                DrawContext v = context.Clone();
                v.Address = addr;
                v.Memory = memory;

                var innerSize = InnerNode.Draw(v, tx, y);
            
                size.Width = Math.Max(size.Width, innerSize.Width + tx - origX);
                size.Height += innerSize.Height;
            }

            return size;
        }

        public override int CalculateDrawnHeight(DrawContext context)
        {
            if (IsHidden && !IsWrapped)
            {
                return HiddenHeight;
            }

            var h = context.Font.Height;
            if (LevelsOpen[context.Level])
            {
                h += InnerNode.CalculateDrawnHeight(context);
            }
            return h;
        }
    }

    public class PS2PtrNode : BaseClassWrapperNode
	{
		private readonly MemoryBuffer memory = new MemoryBuffer();

		public override int MemorySize => 0x4;

		protected override bool PerformCycleCheck => false;

		public override void GetUserInterfaceInfo(out string name, out Image icon)
		{
			name = "PS2 Pointer";
			icon = Properties.Resources.logo_pcsx2;
		}

		public override void Initialize()
		{
			var node = ClassNode.Create();
			node.Initialize();
			node.AddBytes(64);
			ChangeInnerNode(node);
		}

		public override Size Draw(DrawContext context, int x, int y)
		{
			if (IsHidden && !IsWrapped)
				return DrawHidden(context, x, y);

			var origX = x;
			var origY = y;

			AddSelection(context, x, y, context.Font.Height);

			x = AddOpenCloseIcon(context, x, y);
			x = AddIcon(context, x, y, context.IconProvider.Pointer, -1, HotSpotType.None);

			var tx = x;
			x = AddAddressOffset(context, x, y);

			/* RENDER ROW ITEM */
			var ptr = context.Memory.ReadUInt32(Offset);
            x = AddText(context, x, y, context.Settings.TypeColor, HotSpot.NoneId, "PS2Ptr") + context.Font.Width;
			x = AddText(context, x, y, context.Settings.NameColor, HotSpot.NameId, Name) + context.Font.Width;
			x = AddIcon(context, x, y, context.IconProvider.Change, 4, HotSpotType.ChangeClassType);
			x = AddText(context, x, y, Color.Red, HotSpot.NoneId, " -> ");
            x = AddText(context, x, y, context.Settings.ValueColor, HotSpot.AddressId, String.Format("0x{0:X}", ptr));
            x += context.Font.Width;

			AddComment(context, x, y);

			DrawInvalidMemoryIndicatorIcon(context, y);
			AddContextDropDownIcon(context, y);
			AddDeleteIcon(context, y);

			y += context.Font.Height;

			var size = new Size(x - origX, y - origY);

			if (LevelsOpen[context.Level])
			{
				IntPtr addr = context.Address + Offset;
				if (!addr.IsNull())
				{
                    long address = 0;
                    var _uint = context.Process.ReadRemoteUInt32(addr);
                    if (!addr.IsNull() && PS2Helpers.GetEEMem(context, ref address))
                        addr = (IntPtr)(address + _uint);
                }

				memory.Size = InnerNode.MemorySize;
				memory.UpdateFrom(context.Process, addr);

				DrawContext v = context.Clone();
				v.Address = addr;
				v.Memory = memory;

				var innerSize = InnerNode.Draw(v, tx, y);
				size.Width = Math.Max(size.Width, innerSize.Width + tx - origX);
				size.Height += innerSize.Height;
			}

			return size;
		}

		public override int CalculateDrawnHeight(DrawContext context)
		{
			if (IsHidden && !IsWrapped)
			{
				return HiddenHeight;
			}

			var h = context.Font.Height;
			if (LevelsOpen[context.Level])
			{
				h += InnerNode.CalculateDrawnHeight(context);
			}
			return h;
		}
    }

    public class PS2Helpers
    {
        /// IMPORTS
        [Flags]
        public enum MemoryProtectionFlags : uint
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IMAGE_DOS_HEADER
        {
            public short e_magic;
            public short e_cblp;
            public short e_cp;
            public short e_crlc;
            public short e_cparhdr;
            public short e_minalloc;
            public short e_maxalloc;
            public short e_ss;
            public short e_sp;
            public short e_csum;
            public short e_ip;
            public short e_cs;
            public short e_lfarlc;
            public short e_ovno;
            public short e_res_0;
            public short e_res_1;
            public short e_res_2;
            public short e_res_3;
            public short e_oemid;
            public short e_oeminfo;
            public short e_res2_0;
            public short e_res2_1;
            public short e_res2_2;
            public short e_res2_3;
            public short e_res2_4;
            public short e_res2_5;
            public short e_res2_6;
            public short e_res2_7;
            public short e_res2_8;
            public short e_res2_9;
            public int e_lfanew;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IMAGE_DATA_DIRECTORY
        {
            public uint VirtualAddress;
            public uint Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IMAGE_NT_HEADERS
        {
            public uint Signature;
            public IMAGE_FILE_HEADER FileHeader;
            public IMAGE_OPTIONAL_HEADER OptionalHeader;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IMAGE_FILE_HEADER
        {
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct IMAGE_OPTIONAL_HEADER
        {
            public ushort Magic;
            public byte MajorLinkerVersion;
            public byte MinorLinkerVersion;
            public uint SizeOfCode;
            public uint SizeOfInitializedData;
            public uint SizeOfUninitializedData;
            public uint AddressOfEntryPoint;
            public uint BaseOfCode;
            public uint BaseOfData;
            public uint ImageBase;
            public uint SectionAlignment;
            public uint FileAlignment;
            public ushort MajorOperatingSystemVersion;
            public ushort MinorOperatingSystemVersion;
            public ushort MajorImageVersion;
            public ushort MinorImageVersion;
            public ushort MajorSubsystemVersion;
            public ushort MinorSubsystemVersion;
            public uint Win32VersionValue;
            public uint SizeOfImage;
            public uint SizeOfHeaders;
            public uint CheckSum;
            public ushort Subsystem;
            public ushort DllCharacteristics;
            public uint SizeOfStackReserve;
            public uint SizeOfStackCommit;
            public uint SizeOfHeapReserve;
            public uint SizeOfHeapCommit;
            public uint LoaderFlags;
            public uint NumberOfRvaAndSizes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public IMAGE_DATA_DIRECTORY[] DataDirectory;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ExportDirectory
        {
            public uint Characteristics;
            public uint TimeDateStamp;
            public ushort MajorVersion;
            public ushort MinorVersion;
            public uint Name;
            public uint Base;
            public uint NumberOfFunctions;
            public uint NumberOfNames;
            public uint AddressOfFunctions;
            public uint AddressOfNames;
            public uint AddressOfNameOrdinals;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public ExportEntry[] Exports;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct ExportEntry
        {
            public uint NameRVA;
            public uint Ordinal;
            public uint AddressOfData;
            public uint ForwarderRVA;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public MemoryProtectionFlags AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        private static Int64 EEMem = 0;

        public static bool GetEEMem(DrawContext ctx, ref Int64 outBaseAddr)
        {
            Int64 result = EEMem;
            if (result > 0)
            {
                outBaseAddr = result;
                return true;
            }

            if (ctx.Process == null || ctx.Process.IsValid == false)
                return false;

            var procId = ctx.Process.UnderlayingProcess.Id;
            var procName = ctx.Process.UnderlayingProcess.Name;
            if (procName.Count() <= 0)
                return false;

            IntPtr baseAddress = IntPtr.Zero;
            foreach (var proc in ctx.Process.Modules)
            {
                if (proc.Name != procName)
                    continue;

                baseAddress = proc.Start;
                break;
            }
            if (baseAddress.IsNull())
                return false;

            /* Read DOS header */

            //  
            IMAGE_DOS_HEADER dosHeader = ReadStruct<IMAGE_DOS_HEADER>(ctx.Process, baseAddress);
            if (dosHeader.e_magic != 0x5A4D) // 'MZ'
                return false;

            /* Read PE header */
            IMAGE_NT_HEADERS ntHeader = ReadStruct<IMAGE_NT_HEADERS>(ctx.Process, baseAddress + dosHeader.e_lfanew);
            if (ntHeader.Signature != 0x00004550) // 'PE'
                return false;

            /* Enumerate Directories */
            IntPtr EEMemAddr = IntPtr.Zero;
            foreach (IMAGE_DATA_DIRECTORY dir in ntHeader.OptionalHeader.DataDirectory)
            {
                //  Get the address of the export table
                var exportsRva = (IntPtr)(baseAddress.ToInt64() + dir.VirtualAddress);
                if (exportsRva == IntPtr.Zero)
                    continue;

                //  Read the export table
                ExportDirectory exports = ReadStruct<ExportDirectory>(ctx.Process, exportsRva);
                if (exports.NumberOfNames != exports.NumberOfFunctions || exports.AddressOfNames == 0 || exports.AddressOfFunctions == 0 || exports.AddressOfNameOrdinals == 0)
                    continue;

                //  Read address offsets into an array
                uint[] namesRvaArray = ReadArray<uint>(ctx.Process, (IntPtr)(baseAddress.ToInt64() + exports.AddressOfNames), (int)exports.NumberOfNames);
                uint[] functionsRvaArray = ReadArray<uint>(ctx.Process, (IntPtr)(baseAddress.ToInt64() + exports.AddressOfFunctions), (int)exports.NumberOfFunctions);
                ushort[] nameOrdinalsArray = ReadArray<ushort>(ctx.Process, (IntPtr)(baseAddress.ToInt64() + exports.AddressOfNameOrdinals), (int)exports.NumberOfNames);

                //  Iterate names in names array
                uint rva = 0;
                int index = -1;
                foreach (uint nameRva in namesRvaArray)
                {
                    index++;
                    string currentFunctionName = ctx.Process.ReadRemoteStringUntilFirstNullCharacter((IntPtr)(baseAddress.ToInt64() + nameRva), System.Text.Encoding.ASCII, 64); // ReadString((IntPtr)(baseAddress.ToInt64() + nameRva));
                    if (currentFunctionName.ToLower() == "eemem")   //  compare exported name with input
                    {
                        rva = nameRva;
                        break;
                    }
                }

                if (rva == 0)
                    continue;

                //	get function address
                int ordinal_index = nameOrdinalsArray[index];//	get ordinal at the current index
                uint functionRva = functionsRvaArray[ordinal_index]; //	get function va from the ordinal index of the functions array
                if (functionRva <= 0)
                    continue;

                long functionAddress = (long)(baseAddress.ToInt64() + functionRva);
                if (functionAddress <= 0)
                    continue;

                EEMemAddr = (IntPtr)functionAddress;
            }

            if (EEMemAddr == IntPtr.Zero)
                return false;

            result = ctx.Process.ReadRemoteInt64(EEMemAddr); //  read the address of the EEMem function

            outBaseAddr = EEMem;

            EEMem = result;

            return EEMem != 0;
        }

        //  Reads an array of members at the specified address in memory
        private static T[] ReadArray<T>(RemoteProcess proc, IntPtr addr, int size) where T : struct
        {
            int structSize = Marshal.SizeOf(typeof(T)) * size;
            byte[] buffer = proc.ReadRemoteMemory(addr, structSize);

            T[] result = new T[size];
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
            for (int i = 0; i < size; i++)
            {
                Marshal.Copy(buffer, i * Marshal.SizeOf(typeof(T)), ptr, Marshal.SizeOf(typeof(T)));
                result[i] = Marshal.PtrToStructure<T>(ptr);
            }

            return result;
        }

        //  Reads memory at the specified address and transforms the result into the input structure
        private static T ReadStruct<T>(RemoteProcess proc, IntPtr addr) where T : struct
        {
            int structSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = proc.ReadRemoteMemory(addr, structSize);

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
    }
}