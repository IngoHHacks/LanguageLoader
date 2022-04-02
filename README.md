## Language Loader ##
A BepInEx plugin to load languages from csv files.  
Loads languages from .ilang files (formatted in csv) inside the plugins folder.  
The start of .ilang files may contain metadata in ```key: value``` format, terminated by ```#ENDMETA#```.  
An example translation file is provided in the **Example** folder.  
Languages can be selected from the in-game settings.

Supported meta fields:  
- **id** - The namespaced id of the translation.  
- **name** - The displayed name of the translation.  
- **author** - The creator(s) of the language.  
- **special** - Additional properties to be used by LanguageLoader.

Dialogue codes:  
- **[end:]** - Force newline
- **[e:(x)]** - Set emotion to *(x)*
- **[w:(t)]** - Wait for *(t)* seconds
- **[size:(s)]** - Set font size to *(s)*
- **[t:(f)]** - Set character frequency (dialogue speed) to *(f)*
- **[c:(c)]** - Set color to *(c)*
- **[leshy:(d)]** - Makes leshy say *(d)*
- **[shake:(i)]** - Shakes the talking card with intensity *(i)*
- **[anim:(a)]** - Plays animation *(a)*
- **[p:(p)]** - Sets the voice pitch to *(p)*

### Changelog ###

#### 2.0.0
- Update template
- Fixed names for languages without meta

#### 1.0.2
- Fix issue with ```special``` metadata being null

#### 1.0.1
- Fix README