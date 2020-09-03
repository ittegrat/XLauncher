add .net version detection code to setup / xlauncher ==> log
UI -> ControlsList -> revise spacing
set title ??

##  ChangeLog  ##
  ### v0.8.0
    - setup improvements
    - admin functions improvements
    - tiny fixes
  ### v0.7.0
    - generate session
    - animate reload
    - tiny fixes
  ### v0.6.0
    - Admin functions
    - tiny fixes

##  Documentation  ##

  - Tutorial (User / AM)
    - UI + Portable
    - Environment
    - Session (batch)
  - Annotated XSDs


##  UI  ##

  - Check resolved addins paths (setting ?)
  - Generic exceptions => more specific exceptions
  - Revise Global / User settings split --> advanced settings ?

  + Env description --> tooltip (or tab ?; or md renderer ?)
  + Env help / contact list
  + Update over http
  + UI help
  + Automatic overlays (e.g. @*.xml, #*.xml, ...)

  // - Configuration

  - Environments

    - EnvList context menu
      - Export impl
      - import impl

    // - Global Addins

    // - Parameters

    // - Addins

    // - Variables

  - Settings
    - Local root context menu
    - ...

  - Logs
    - listview with time / level / message; color trigger on level; config log level -> Info
      if enhanced add column for callsite or show full log in editor


##  XAI  ##

  - build 32 + 64 xll
  - com / capi load --> session option ? addin option ?
  - param table --> check size
  - error messages
  + resolution of addins with relative path (envvar ? framework param? resolved in UI / Entities?)


##  UI Setup  ##

  // - ...


##  Entities  ##

  - Generic exceptions => more specific exceptions

  - Authentication
    + maybe: wildcars

  // - Environmens


## Specs ##

### Tree Structure ###
  Root
   + Environments
   |  + User1
   |  |  + _Environment.xml
   |  |  + ...
   |  + User2
   |  |  + _Environment.xml
   |  |  + ...
   + Settings
   |  + Settings.xml
   |  + PubEnvPrefs1.xml
   |  + ...
   + XLauncher.exe
   + XLauncher.exe.config
   + XLauncher32.xll
   + XLauncher32.xll.config
   + XLauncher64.xll
   + XLauncher64.xll.config
   + *.dll

### Exception and log messages ###
  - exception messages => final point
  - log messages => final point
  - exception log messages => NO final point


Add-Ins loading order
1) Quelle specificate in Tools -> AddIns
2) Explorer, Launcher, Linea di comando...

In caso di caricamento di versioni diverse della medesima add-in, se
il nome dell'XLL e' lo stesso, l'apertura del secondo file provoca
la ri-esecuzione della funzione xlAutoOpen del modulo caricato in
precedenza; se il nome dell'immagine e' differente, viene eseguita
la xlAutoOpen corretta ed i nomi di funzione gia' registrati vengono
sovrascritti.
