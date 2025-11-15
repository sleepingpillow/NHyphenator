# NHyphenator

C# implementation of Frank Liang's hyphenation algorithm (also known as Knuth-Liang algorithm).
Read more about algorithm on http://en.wikipedia.org/wiki/Hyphenation_algorithm

This implementation contains original TEX hyphenation patterns (see http://tug.org/tex-hyphen/) for British and American English, and Russian language 

## Requirements

- .NET 10.0 or later

## NuGet

https://www.nuget.org/packages/NHyphenator/

## Example

### Simple usage example:

```c#
var loader = new ResourceHyphenatePatternsLoader(HyphenatePatternsLanguage.Russian);
Hypenator hypenator = new Hyphenator(loader, "-");
var result = hypenator.HyphenateText(text);
```

### Adding new languages

This library contains build-in patterns for English and Russian languages (stored in .resx file)

You can add (or update) language patterns through using FilePatternsLoader and load patterns from files

```csharp
var loader = new new FilePatternsLoader($"{patterns_path}", $"{exceptions_path}");
```

Also you can create own implementation of `IHyphenatePatternsLoader` interface

You can find patterns [here](https://github.com/hyphenation/tex-hyphen/tree/master/hyph-utf8/tex/generic/hyph-utf8/patterns/txt):
`.pat.txt` files contain patterns, `.hyp.txt` files contain exceptions

## Performance

The library has been heavily optimized for .NET 10 with dramatic performance improvements:
- **~96% reduction in memory allocations**
- **~50% faster execution time**
- Efficient use of modern .NET APIs (ArrayPool, Span<T>, string.Create)

For performance benchmarks and optimization plans, see:
- [Benchmarks](NHyphenator.Benchmarks/README.md)
- [Optimization Plan](OPTIMIZATION_PLAN.md)

## Licence

Source code are distributed under Apache 2.0 licence. 
Hyphenation patterns are distributed under LaTeX Project Public License.

A bit more information you can find in my blog http://alkozko.ru/blog/post/NHyphenator-en

## Russian descripton

Подробнее о библиотеке можно прочесть (на русском) в моем блоге http://alkozko.ru/Blog/Post/liang-hyphenation-algorithm-on-c-sharp и http://alkozko.ru/blog/post/nhyphenator-12
