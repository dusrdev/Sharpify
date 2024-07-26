# CHANGELOG

## v2.2.0

* `SortedList`
  * `GetIndex` now is not simplified, instead returns pure result of performing a binary search:
    * if the item exists, a zero based index
    * if not, a negative number that is the bitwise complement of the index of the next element that is larger than the item or, if there is no larger element, the bitwise complement of `Count` property.
    * With this you can still find out if something exists by checking for negative, but you can now also use the negative bitwise complement of the index to get section of the list in relation to the item, best used with the span.
  * Added `AddRange` functions for `ReadOnlySpan{T}` and `IEnumerable{T}`
* Collection extensions `ToArrayFast` and `ToListFast` were removed after being marked as `Obsolete` in the previous version, the `Linq` alternatives perform almost the same, consider using them instead.

### Deprecation

A lot of features of the package are already deprecated, and some will continue in this direction as Microsoft seems to really focus on improving performance in the latest versions of C#, some methods that this package originally offered to provide a better performance than what is already in core language, will instead provide worse performance, at which point they've served their purpose.

As such, in the next major versions, many of these features will be removed. To avoid breaking old codebases, for example for `.NET 7` users, the best way forward would be to stop supporting this `.NET` version in the future versions of `Sharpify`, this means that existing users will still have what they used, but the users of the newer technologies will be able to use newer versions of `Sharpify` that rely on said technologies to remain dynamic, and stay ahead on the performance curve. This is the only way that I will be able to implement new features, without worrying about multiple implementations of each feature for each version of `.NET` just to maintain compatibility.

The coming release of `.NET 9` and `C# 13` seem like the natural point to start this next chapter, I would've loved to play with the new extension functionality, but seems it will be delayed further. In any case, future features of this package will need to rely on future features from `.NET`, therefore it makes no sense to keep trying to add support for them in older versions as well.
