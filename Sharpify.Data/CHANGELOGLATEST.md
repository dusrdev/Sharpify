# CHANGELOG

* **BREAKING** `UpsertAsT` functions signature changed to force usage of new parameter, which is a `JsonSerializerContext` that can serialize `T`.
* Simplified structures of inner data representation, slightly reducing assembly size.
* `Sharpify.Data` is now fully AOT compatible
