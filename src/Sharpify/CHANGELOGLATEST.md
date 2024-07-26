# CHANGELOG

## v2.2.0

* `SortedList`
  * `GetIndex` now is not simplified, instead returns pure result of performing a binary search:
    * if the item exists, a zero based index
    * if not, a negative number that is the bitwise complement of the index of the next element that is larger than the item or, if there is no larger element, the bitwise complement of `Count` property.
    * With this you can still find out if something exists by checking for negative, but you can now also use the negative bitwise complement of the index to get section of the list in relation to the item, best used with the span.
  * Added `AddRange` functions for `ReadOnlySpan{T}` and `IEnumerable{T}`
* Collection extensions `ToArrayFast` and `ToListFast` were removed after being marked as `Obsolete` in the previous version, the `LINQ` alternatives perform only marginally slower, but will improve passively under the hood, consider using them instead.
