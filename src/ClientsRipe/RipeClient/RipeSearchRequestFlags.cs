namespace ClientsRipe
{
    public enum RipeSearchRequestFlags
    {
        /// <summary>
        /// None flags
        /// </summary>
        None,
        /// <summary>
        /// -M (or --all-more) - requests that the server should return all the sub-ranges
        /// that are completely contained within the reference range. When there are
        /// hierarchies of sub-ranges, all levels of the hierarchies will be returned down
        /// to the smallest sub-range in each hierarchy. Many objects can be returned.
        /// </summary>
        AllMore,
        /// <summary>
        /// -m (or --one-more) - requests that the server should return all the sub-ranges
        /// that are completely contained within the reference range. When there are
        /// hierarchies of sub-ranges, only the top level of the hierarchies will be returned.
        /// These are usually called one level more specific ranges. Many objects can be returned.
        /// </summary>
        OneMore,
        
        /// <summary>
        /// -L (or --all-less) - requests that the server returns the object with an exact match
        /// range, if it exists, and all objects with ranges that are bigger than the reference
        /// range and that fully contain, or encompass, it. Many objects can be returned.
        /// </summary>
        AllLess,
        /// <summary>
        /// -l (or --one-less) - requests that the server does not return the object with an exact
        /// match range, if it exists. It returns only the object with the smallest range that is
        /// bigger than the reference range and that fully contains, or encompasses, it. This is
        /// usually referred to as the one level less specific range. Only one object can be
        /// returned. Less Specific Range Queries For Referenced IRT Objects
        /// </summary>
        OneLess,
    }
}
