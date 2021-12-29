<!-- Replace this line with what this PR does and why.  Describe what you'd like reviewers to know, how you applied the Engineering principles, and any interesting tradeoffs made.  Delete bullet points below that don't apply, and update the changelog section as appropriate. -->

<!-- Add JIRA link here. Short version (e.g. MTT-123) also works and gets auto-linked. -->

<!-- Add RFC link here if applicable. -->

<!-- Original PR link if this is a backport PR -->

<!-- When backporting is required please add the type:backport-release-<version> label to this PR. This should include all versions in which this should be backported to. -->

### PR Checklist
- [ ] Have you added a backport label (if needed)? For example, the `type:backport-release-*` label. After you backport the PR, the label changes to `stat:backported-release-*`.
- [ ] Have you updated the changelog? Each package has a `CHANGELOG.md` file.
- [ ] Have you updated or added the documentation for your PR? When you add a new feature, change a property name, or change the behavior of a feature, it's best practice to include related documentation changes in the same PR or a link to the documenation repo PR if this is a manual update. 

## Changelog

### com.unity.netcode.gameobjects
- Added: The package whose Changelog should be added to should be in the header. Delete the changelog section entirely if it's not needed.
- Fixed: If you update multiple packages, create a new section with a new header for the other package. 
- Removed/Deprecated/Changed: Each bullet should be prefixed with Added, Fixed, Removed, Deprecated, or Changed to indicate where the entry should go.

## Testing and Documentation

* No tests have been added.
* Includes unit tests.
* Includes integration tests.
* No documentation changes or additions were necessary.
* Includes documentation for previously-undocumented public API entry points.
* Includes edits to existing public API documentation.

<!--  Uncomment and mark items off with a * if this PR deprecates any API:
### Deprecated API
- [ ] An `[Obsolete]` attribute was added along with a `(RemovedAfter yyyy-mm-dd)` entry.
- [ ] An [api updater] was added.
- [ ] Deprecation of the API is explained in the CHANGELOG.
- [ ] The users can understand why this API was removed and what they should use instead.
-->
