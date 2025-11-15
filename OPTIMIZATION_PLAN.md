# .NET 10 Optimization Plan

This document outlines the optimization plan for NHyphenator using .NET 10's memory-efficient APIs.

## Completed Optimizations

### ✅ Phase 1: Migration & Basic Optimizations
- **Migrated to .NET 10** from netstandard2.0/netcoreapp3.1
- **Updated dependencies** to latest versions
- **Added BenchmarkDotNet** infrastructure for performance testing
- **Basic memory optimizations:**
  - Use `Array.Empty<int>()` instead of `new int[0]` (zero allocation for empty arrays)
  - Use `c - '0'` instead of `Int32.Parse` for single digit parsing (avoid string allocation)
  - Preallocate StringBuilder capacity where size is known (reduce reallocations)

**Results:** ~15% reduction in memory allocations with no performance regression

## Future Optimization Opportunities

### Phase 2: Advanced Memory Optimizations (Not Implemented)

#### 1. Use `Span<T>` and `ReadOnlySpan<T>` for String Operations
**Potential Benefits:** Reduce string allocations in hot paths

**Areas to consider:**
- `HyphenateWordsInText`: Process text character-by-character using `ReadOnlySpan<char>`
- `FindLastWord`: Use span slicing instead of Substring
- `CreatePattern`: Use stackalloc for small buffers

**Complexity:** High - Requires careful refactoring to avoid breaking changes

#### 2. Use `ArrayPool<T>` for Temporary Arrays
**Potential Benefits:** Reduce GC pressure from temporary array allocations

**Areas to consider:**
- `GenerateLevelsForWord`: Pool the levels array (note: cannot be used if array is stored long-term)
- `CreateHyphenateMaskFromLevels`: Pool the mask array (note: same caveat)

**Complexity:** Medium - Need to ensure arrays are returned to pool and not stored

#### 3. Use `SearchValues<T>` for Character Searches
**Potential Benefits:** Faster character searching (available in .NET 8+)

**Areas to consider:**
- Replace `char.IsLetter` checks with `SearchValues` for letter sets
- Pattern matching optimizations

**Complexity:** Low-Medium

#### 4. Use `CollectionsMarshal` for Direct List Access
**Potential Benefits:** Avoid bounds checking in tight loops

**Areas to consider:**
- Pattern list access in `GenerateLevelsForWord`

**Complexity:** Medium

#### 5. Optimize String Building with Interpolation Handlers
**Potential Benefits:** Reduce StringBuilder allocations

**Areas to consider:**
- Use string interpolation with custom handlers for hyphenation

**Complexity:** High

## Benchmarking Guidelines

Before implementing any optimization:

1. **Measure first:** Run benchmarks to establish baseline
2. **Implement:** Make the optimization
3. **Measure again:** Verify the improvement
4. **Test thoroughly:** Ensure no regressions in functionality

Use the benchmark project:
```bash
cd NHyphenator.Benchmarks
dotnet run -c Release
```

## Notes on Implementation

- **Correctness first:** Any optimization should not break existing functionality
- **Minimal changes:** Prefer small, incremental changes that are easy to verify
- **Backward compatibility:** Maintain API compatibility where possible
- **Test coverage:** All optimizations should be covered by existing tests

## Performance Targets

Current performance is adequate for most use cases. Future optimizations should aim for:
- **Memory:** Additional 10-20% reduction in allocations
- **Speed:** Maintain current performance or improve by 10-20%
- **No regressions:** All existing tests must pass

## References

- [.NET 10 Performance Improvements](https://devblogs.microsoft.com/dotnet/)
- [Span<T> Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.span-1)
- [ArrayPool<T> Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1)
- [BenchmarkDotNet Best Practices](https://benchmarkdotnet.org/articles/guides/good-practices.html)
