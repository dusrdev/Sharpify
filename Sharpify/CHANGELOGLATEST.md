# CHANGELOG

## v1.7.2

* Moved `_lock` acquiring statements into the `try-finally` blocks to handle `out-of-band` exceptions. Thanks [TheGenbox](https://www.reddit.com/user/TheGenbox/).
* Modified most of the internal `await` statements to use `ConfigureAwait(false)` wherever it was possible, also thanks to [TheGenbox](https://www.reddit.com/user/TheGenbox/).
