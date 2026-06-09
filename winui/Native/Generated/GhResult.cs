namespace StarSorter.Native
{
    public unsafe partial struct GhResult
    {
        [NativeTypeName("char *")]
        public sbyte* data;

        public int error_code;
    }
}
