
## Load Environment

  1. deserialize _Environment.xml
  2. foreach 'auth' reverse merge
  3. foreach 'import'
       break if 'after'
       merge 'Framework' || 'Environment' withAuth
  4. foreach [^\._] *.xml merge 'Framework'
  5. foreach remaining 'import'
       merge 'Framework' || 'Environment' withAuth


### AuthDB Merge
Case insensitive
All --> no children
foreach other 'Domain.Key' merge or add
  if All || other.All return
  foreach other 'User' merge or add
    if All || other.All return
    foreach other 'Machine' add

### Environment Merge
if 'withAuth' merge AuthDB
foreach 'Framework' merge

### Framework Merge
Addin.Id --> [Arch]{Key || FileName} case insensitive
Control.Name unique
if !exist
  insert or add
else
  foreach other 'EVar' replace or add
  foreach other 'Addin.Id' replace or add
  foreach other 'Box'
    foreach other 'Control'
      if exist --> replace + remove
      if box exist --> add + remove
    if box !empty --> add


## IsAuthorized
if denyDomain
  if denyDomain.All --> false
  if denyUser
    if denyUser.All --> false
    if denyMachine --> false

if allowDomain
  if allowDomain.All --> true
  if allowUser
    if allowUser.All --> true
    if allowMachine --> true

--> false
