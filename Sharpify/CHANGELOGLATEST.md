# CHANGELOG

* Added `AppendLine` overloads to all `Append` methods of the `StringBuffer` variants that append the platform specific new line sequence at after the `Append`, there is also an empty overload which just appends the new line sequence. This was made to reduce code when using newlines, and to make the api even more similar to `StringBuilder`.
