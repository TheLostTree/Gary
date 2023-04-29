using System.Runtime.CompilerServices;
using ImGuiNET;

namespace Gary;
// public struct ImGuiListClipper
// {
//   public IntPtr Ctx;
//   public int DisplayStart;
//   public int DisplayEnd;
//   public int ItemsCount;
//   public float ItemsHeight;
//   public float StartPosY;
//   public unsafe void* TempData;
// }
public struct ImGuiListClipperPtr {
    public unsafe ImGuiListClipper* NativePtr { get; }

    public unsafe ImGuiListClipperPtr(ImGuiListClipper* nativePtr) => this.NativePtr = nativePtr;

    public unsafe ImGuiListClipperPtr(IntPtr nativePtr) => this.NativePtr = (ImGuiListClipper*) (void*) nativePtr;

    public static unsafe implicit operator ImGuiListClipperPtr(ImGuiListClipper* nativePtr) => new ImGuiListClipperPtr(nativePtr);

    public static unsafe implicit operator ImGuiListClipper*(ImGuiListClipperPtr wrappedPtr) => wrappedPtr.NativePtr;

    public static implicit operator ImGuiListClipperPtr(IntPtr nativePtr) => new ImGuiListClipperPtr(nativePtr);

    public unsafe ref int DisplayStart => ref Unsafe.AsRef<int>((void*) &this.NativePtr->DisplayStart);

    public unsafe ref int DisplayEnd => ref Unsafe.AsRef<int>((void*) &this.NativePtr->DisplayEnd);

    public unsafe ref int ItemsCount => ref Unsafe.AsRef<int>((void*) &this.NativePtr->ItemsCount);

    public unsafe ref float ItemsHeight => ref Unsafe.AsRef<float>((void*) &this.NativePtr->ItemsHeight);

    public unsafe ref float StartPosY => ref Unsafe.AsRef<float>((void*) &this.NativePtr->StartPosY);

    public unsafe IntPtr TempData
    {
      get => (IntPtr) this.NativePtr->TempData;
      set => this.NativePtr->TempData = (void*) value;
    }

    public unsafe void Begin(int items_count)
    {
      float items_height = -1f;
      ImGuiNative.ImGuiListClipper_Begin(this.NativePtr, items_count, items_height);
    }

    public unsafe void Begin(int items_count, float items_height) => ImGuiNative.ImGuiListClipper_Begin(this.NativePtr, items_count, items_height);

    public unsafe void Destroy() => ImGuiNative.ImGuiListClipper_destroy(this.NativePtr);

    public unsafe void End() => ImGuiNative.ImGuiListClipper_End(this.NativePtr);

    public unsafe void ForceDisplayRangeByIndices(int item_min, int item_max) => ImGuiNative.ImGuiListClipper_ForceDisplayRangeByIndices(this.NativePtr, item_min, item_max);

    public unsafe bool Step() => ImGuiNative.ImGuiListClipper_Step(this.NativePtr) > (byte) 0;
  }