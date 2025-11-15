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

### ✅ Phase 2: Advanced Memory Optimizations

#### ✅ Phase 2.1: Use `string.Create` for HyphenateByMask
**Implementation:**
- Replaced StringBuilder with `string.Create` for zero-copy string building
- Added early return when no hyphens are needed
- Uses tuple state to pass context into the lambda

**Results:** 0.9-1.5% additional memory reduction

#### ✅ Phase 2.2: Use `CollectionsMarshal` for Pattern List Access  
**Implementation:**
- Used `CollectionsMarshal.AsSpan` to get direct access to list's underlying array
- Eliminates bounds checking in tight loops within GenerateLevelsForWord

**Results:** Slight speed improvement, memory stable

#### ✅ Phase 2.3: Optimize wordString Creation
**Implementation:**
- Replaced `StringBuilder().Append(Marker).Append(word).Append(Marker)` with `string.Create`
- Eliminates StringBuilder allocation for marker+word+marker concatenation

**Results:** 0.9-1.4% additional memory reduction

**Phase 2 Cumulative Results:** ~17-18% total reduction from original baseline
- Single word: 10.76 KB (was 13.02 KB)
- Short text: 38.59 KB (was 46.56 KB)  
- Long text: 632.19 KB (was 768.52 KB)

### ✅ Phase 3: Advanced Span<T> and ArrayPool Optimizations (COMPLETED)

**Implementation Date:** November 2025

#### ✅ 3.1: ArrayPool<T> for Temporary Arrays
**Implementation:**
- Used `ArrayPool<int>.Shared` for levels and mask arrays
- Proper tracking of actual array lengths vs rented lengths
- Ensured arrays are returned to pool after use

**Results:** Foundation for memory reduction

#### ✅ 3.2: ReadOnlySpan<char> for Pattern Matching  
**Implementation:**
- Eliminated substring allocations in `GenerateLevelsForWord`
- Added `Pattern.Compare(ReadOnlySpan<char>, Pattern)` overloads
- Used span slicing instead of `Substring(i, count)`
- Created helper method `FindPatternIndex` to avoid lambda capture of spans

**Results:** Massive performance improvements

**Phase 3 Cumulative Results:** ~50% faster, ~95% memory reduction
- Single word: 53.91 μs / 520 B (was 113.1 μs / 10.76 KB) - **-52.3% time, -95.2% memory**
- Short text: 203.44 μs / 2.42 KB (was 410.3 μs / 38.59 KB) - **-50.4% time, -93.7% memory**
- Long text: 3,192 μs / 27.34 KB (was 6,387 μs / 632.19 KB) - **-50.0% time, -95.7% memory**

**Comparison to Original Baseline (before Phase 1):**
- Single word: 53.91 μs / 520 B (was 113.1 μs / 13.02 KB) - **-52.3% time, -96.0% memory**
- Short text: 203.44 μs / 2.42 KB (was 410.3 μs / 46.56 KB) - **-50.4% time, -94.8% memory**
- Long text: 3,192 μs / 27.34 KB (was 6,387 μs / 768.52 KB) - **-50.0% time, -96.4% memory**

## Future Optimization Opportunities

### Phase 4: Additional Optimizations (Optional)

#### 1. Use `SearchValues<T>` for Character Searches
**Potential Benefits:** Faster character searching (available in .NET 8+)

**Areas to consider:**
- Replace `char.IsLetter` checks with `SearchValues` for letter sets
- Pattern matching optimizations

**Complexity:** Low-Medium
**Status:** Deferred - Current performance is excellent

#### 2. Use `ReadOnlySpan<char>` in HyphenateWordsInText
**Potential Benefits:** Further reduce string allocations during text processing

**Areas to consider:**
- Process text character-by-character using `ReadOnlySpan<char>`
- `FindLastWord`: Use span slicing instead of Substring

**Complexity:** Medium - Would require API changes
**Status:** Deferred - ~95% memory reduction already achieved

#### 3. Use `CollectionsMarshal` for Direct List Access (Already Done)
**Status:** ✅ Completed in Phase 2.2

#### 4. Optimize String Building with Interpolation Handlers
**Potential Benefits:** Reduce StringBuilder allocations

**Areas to consider:**
- Use string interpolation with custom handlers for hyphenation

**Complexity:** High
**Status:** Deferred - Already optimized with `string.Create`

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

Current performance far exceeds initial targets:

**Initial Targets (Phase 3):**
- **Memory:** Additional 10-20% reduction in allocations
- **Speed:** Maintain current performance or improve by 10-20%
- **No regressions:** All existing tests must pass

**Actual Achievements (Phase 3):**
- **Memory:** ~95% reduction in allocations (far exceeds 10-20% goal)
- **Speed:** ~50% improvement (far exceeds 10-20% goal)
- **No regressions:** ✅ All existing tests pass

**Overall Performance (All Phases Combined):**
From original baseline to current:
- **Memory:** ~96% reduction in allocations
- **Speed:** ~50% faster execution
- **Stability:** All tests passing, zero security issues

## References

- [.NET 10 Performance Improvements](https://devblogs.microsoft.com/dotnet/)
- [Span<T> Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.span-1)
- [ArrayPool<T> Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1)
- [BenchmarkDotNet Best Practices](https://benchmarkdotnet.org/articles/guides/good-practices.html)
