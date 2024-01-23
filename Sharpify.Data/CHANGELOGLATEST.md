# CHANGELOG

## v2.0.2

* Moved `_lock` acquiring statements into the `try-finally` blocks to handle `out-of-band` exceptions. Thanks [TheGenbox](https://www.reddit.com/user/TheGenbox/).
* Modified all internal `await` statements to use `ConfigureAwait(false)`, also thanks to [TheGenbox](https://www.reddit.com/user/TheGenbox/).
