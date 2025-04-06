# ContentBuildSystem
 A toolkit to convert source asset files into a format ready for your engine to use.

## Idea

Convert source asset files (like png) into some compressed/compiled output asset file in your custom format.

Conversion is rule based.

Rules apply to sets of data (example folder with sprites for a given atlas)

## TODO

- [ ] Docs
  - [ ] Describe basic premise
  - [ ] Use-case breakdown
  - [ ] How to use?
  - [ ] How to build plugins?
- [ ] CLI
  - [ ] Parametrize for output folders
- [ ] Build
  - [ ] Ignore already used folders (unless group option is set)
  - [ ] Ignore already processed files (unless group option is set)
  - [ ] Explicit ignore list
- [ ] Monitor
  - [ ] Fix false positive after first build process is done
  - [ ] Use groups to narrow monitoring scope
  - [ ] Allow per-group monitoring
- [ ] Copy action
  - [ ] Add renaming options
  - [ ] Add extension case options

