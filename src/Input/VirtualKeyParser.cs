using System;

namespace WinKeysRemapper.Input
{
    public static class VirtualKeyParser
    {
        /// <summary>
        /// Converts a string key name to a Windows virtual key code.
        /// Supports a comprehensive list of common keys, function keys, arrows, numbers, letters, etc.
        /// NOTE: This method expects pre-normalized input (uppercase and trimmed) for optimal performance.
        /// The ConfigurationManager handles normalization during config loading.
        /// </summary>
        /// <param name="keyName">The normalized string name of the key (uppercase, trimmed)</param>
        /// <param name="virtualKey">The virtual key code if parsing succeeds</param>
        /// <returns>True if the key was successfully parsed, false otherwise</returns>
        public static bool TryParseVirtualKey(string keyName, out int virtualKey)
        {
            virtualKey = 0;
            
            if (string.IsNullOrWhiteSpace(keyName))
                return false;
                
            // Handle all supported keys - input is expected to be pre-normalized
            switch (keyName)
            {
                // Letters A-Z
                case "A": virtualKey = 0x41; return true;
                case "B": virtualKey = 0x42; return true;
                case "C": virtualKey = 0x43; return true;
                case "D": virtualKey = 0x44; return true;
                case "E": virtualKey = 0x45; return true;
                case "F": virtualKey = 0x46; return true;
                case "G": virtualKey = 0x47; return true;
                case "H": virtualKey = 0x48; return true;
                case "I": virtualKey = 0x49; return true;
                case "J": virtualKey = 0x4A; return true;
                case "K": virtualKey = 0x4B; return true;
                case "L": virtualKey = 0x4C; return true;
                case "M": virtualKey = 0x4D; return true;
                case "N": virtualKey = 0x4E; return true;
                case "O": virtualKey = 0x4F; return true;
                case "P": virtualKey = 0x50; return true;
                case "Q": virtualKey = 0x51; return true;
                case "R": virtualKey = 0x52; return true;
                case "S": virtualKey = 0x53; return true;
                case "T": virtualKey = 0x54; return true;
                case "U": virtualKey = 0x55; return true;
                case "V": virtualKey = 0x56; return true;
                case "W": virtualKey = 0x57; return true;
                case "X": virtualKey = 0x58; return true;
                case "Y": virtualKey = 0x59; return true;
                case "Z": virtualKey = 0x5A; return true;
                
                // Numbers 0-9
                case "0": virtualKey = 0x30; return true;
                case "1": virtualKey = 0x31; return true;
                case "2": virtualKey = 0x32; return true;
                case "3": virtualKey = 0x33; return true;
                case "4": virtualKey = 0x34; return true;
                case "5": virtualKey = 0x35; return true;
                case "6": virtualKey = 0x36; return true;
                case "7": virtualKey = 0x37; return true;
                case "8": virtualKey = 0x38; return true;
                case "9": virtualKey = 0x39; return true;
                
                // Function Keys F1-F24
                case "F1": virtualKey = 0x70; return true;
                case "F2": virtualKey = 0x71; return true;
                case "F3": virtualKey = 0x72; return true;
                case "F4": virtualKey = 0x73; return true;
                case "F5": virtualKey = 0x74; return true;
                case "F6": virtualKey = 0x75; return true;
                case "F7": virtualKey = 0x76; return true;
                case "F8": virtualKey = 0x77; return true;
                case "F9": virtualKey = 0x78; return true;
                case "F10": virtualKey = 0x79; return true;
                case "F11": virtualKey = 0x7A; return true;
                case "F12": virtualKey = 0x7B; return true;
                case "F13": virtualKey = 0x7C; return true;
                case "F14": virtualKey = 0x7D; return true;
                case "F15": virtualKey = 0x7E; return true;
                case "F16": virtualKey = 0x7F; return true;
                case "F17": virtualKey = 0x80; return true;
                case "F18": virtualKey = 0x81; return true;
                case "F19": virtualKey = 0x82; return true;
                case "F20": virtualKey = 0x83; return true;
                case "F21": virtualKey = 0x84; return true;
                case "F22": virtualKey = 0x85; return true;
                case "F23": virtualKey = 0x86; return true;
                case "F24": virtualKey = 0x87; return true;
                
                // Arrow Keys
                case "LEFT":
                case "LEFTARROW":
                case "ARROWLEFT":
                    virtualKey = 0x25; return true; // VK_LEFT
                case "UP":
                case "UPARROW":
                case "ARROWUP":
                    virtualKey = 0x26; return true; // VK_UP
                case "RIGHT":
                case "RIGHTARROW":
                case "ARROWRIGHT":
                    virtualKey = 0x27; return true; // VK_RIGHT
                case "DOWN":
                case "DOWNARROW":
                case "ARROWDOWN":
                    virtualKey = 0x28; return true; // VK_DOWN
                
                // Navigation Keys
                case "HOME": virtualKey = 0x24; return true; // VK_HOME
                case "END": virtualKey = 0x23; return true; // VK_END
                case "PAGEUP":
                case "PGUP":
                case "PRIOR": virtualKey = 0x21; return true; // VK_PRIOR
                case "PAGEDOWN":
                case "PGDN":
                case "NEXT": virtualKey = 0x22; return true; // VK_NEXT
                case "INSERT":
                case "INS": virtualKey = 0x2D; return true; // VK_INSERT
                case "DELETE":
                case "DEL": virtualKey = 0x2E; return true; // VK_DELETE
                
                // Modifier Keys
                case "CTRL":
                case "CONTROL":
                case "LCTRL":
                case "LCONTROL": virtualKey = 0xA2; return true; // VK_LCONTROL
                case "RCTRL":
                case "RCONTROL": virtualKey = 0xA3; return true; // VK_RCONTROL
                case "SHIFT":
                case "LSHIFT": virtualKey = 0xA0; return true; // VK_LSHIFT
                case "RSHIFT": virtualKey = 0xA1; return true; // VK_RSHIFT
                case "ALT":
                case "LALT":
                case "LMENU": virtualKey = 0xA4; return true; // VK_LMENU
                case "RALT":
                case "RMENU":
                case "ALTGR": virtualKey = 0xA5; return true; // VK_RMENU
                case "WIN":
                case "WINDOWS":
                case "LWIN": virtualKey = 0x5B; return true; // VK_LWIN
                case "RWIN": virtualKey = 0x5C; return true; // VK_RWIN
                
                // Special Keys
                case "SPACE":
                case "SPACEBAR": virtualKey = 0x20; return true; // VK_SPACE
                case "ENTER":
                case "RETURN": virtualKey = 0x0D; return true; // VK_RETURN
                case "BACKSPACE":
                case "BACK": virtualKey = 0x08; return true; // VK_BACK
                case "TAB": virtualKey = 0x09; return true; // VK_TAB
                case "ESCAPE":
                case "ESC": virtualKey = 0x1B; return true; // VK_ESCAPE
                case "CAPSLOCK":
                case "CAPS": virtualKey = 0x14; return true; // VK_CAPITAL
                case "NUMLOCK": virtualKey = 0x90; return true; // VK_NUMLOCK
                case "SCROLLLOCK":
                case "SCROLL": virtualKey = 0x91; return true; // VK_SCROLL
                case "PRINTSCREEN":
                case "PRINT":
                case "PRTSC": virtualKey = 0x2C; return true; // VK_SNAPSHOT
                case "PAUSE": virtualKey = 0x13; return true; // VK_PAUSE
                case "BREAK": virtualKey = 0x03; return true; // VK_CANCEL
                
                // Numpad Keys
                case "NUMPAD0":
                case "NUM0": virtualKey = 0x60; return true; // VK_NUMPAD0
                case "NUMPAD1":
                case "NUM1": virtualKey = 0x61; return true; // VK_NUMPAD1
                case "NUMPAD2":
                case "NUM2": virtualKey = 0x62; return true; // VK_NUMPAD2
                case "NUMPAD3":
                case "NUM3": virtualKey = 0x63; return true; // VK_NUMPAD3
                case "NUMPAD4":
                case "NUM4": virtualKey = 0x64; return true; // VK_NUMPAD4
                case "NUMPAD5":
                case "NUM5": virtualKey = 0x65; return true; // VK_NUMPAD5
                case "NUMPAD6":
                case "NUM6": virtualKey = 0x66; return true; // VK_NUMPAD6
                case "NUMPAD7":
                case "NUM7": virtualKey = 0x67; return true; // VK_NUMPAD7
                case "NUMPAD8":
                case "NUM8": virtualKey = 0x68; return true; // VK_NUMPAD8
                case "NUMPAD9":
                case "NUM9": virtualKey = 0x69; return true; // VK_NUMPAD9
                case "NUMPADMULTIPLY":
                case "MULTIPLY":
                case "NUM*": virtualKey = 0x6A; return true; // VK_MULTIPLY
                case "NUMPADADD":
                case "ADD":
                case "NUM+": virtualKey = 0x6B; return true; // VK_ADD
                case "NUMPADSUBTRACT":
                case "SUBTRACT":
                case "NUM-": virtualKey = 0x6D; return true; // VK_SUBTRACT
                case "NUMPADDECIMAL":
                case "DECIMAL":
                case "NUM.": virtualKey = 0x6E; return true; // VK_DECIMAL
                case "NUMPADDIVIDE":
                case "DIVIDE":
                case "NUM/": virtualKey = 0x6F; return true; // VK_DIVIDE
                case "NUMPADENTER": virtualKey = 0x0D; return true; // Same as regular Enter
                
                // Symbol Keys (common punctuation)
                case "SEMICOLON":
                case ";": virtualKey = 0xBA; return true; // VK_OEM_1 (;:)
                case "EQUALS":
                case "=": virtualKey = 0xBB; return true; // VK_OEM_PLUS (=+)
                case "COMMA":
                case ",": virtualKey = 0xBC; return true; // VK_OEM_COMMA (,<)
                case "MINUS":
                case "DASH":
                case "-": virtualKey = 0xBD; return true; // VK_OEM_MINUS (-)
                case "PERIOD":
                case "DOT":
                case ".": virtualKey = 0xBE; return true; // VK_OEM_PERIOD (.>)
                case "SLASH":
                case "FORWARDSLASH":
                case "/": virtualKey = 0xBF; return true; // VK_OEM_2 (/?)
                case "BACKTICK":
                case "GRAVE":
                case "`": virtualKey = 0xC0; return true; // VK_OEM_3 (`~)
                case "LEFTBRACKET":
                case "OPENBRACKET":
                case "[": virtualKey = 0xDB; return true; // VK_OEM_4 ([{)
                case "BACKSLASH":
                case "\\": virtualKey = 0xDC; return true; // VK_OEM_5 (\|)
                case "RIGHTBRACKET":
                case "CLOSEBRACKET":
                case "]": virtualKey = 0xDD; return true; // VK_OEM_6 (]})
                case "QUOTE":
                case "APOSTROPHE":
                case "'": virtualKey = 0xDE; return true; // VK_OEM_7 ('")
                
                // Media Keys
                case "VOLUMEUP":
                case "VOLUP": virtualKey = 0xAF; return true; // VK_VOLUME_UP
                case "VOLUMEDOWN":
                case "VOLDOWN": virtualKey = 0xAE; return true; // VK_VOLUME_DOWN
                case "VOLUMEMUTE":
                case "MUTE": virtualKey = 0xAD; return true; // VK_VOLUME_MUTE
                case "MEDIANEXT":
                case "NEXTTRACK": virtualKey = 0xB0; return true; // VK_MEDIA_NEXT_TRACK
                case "MEDIAPREV":
                case "MEDIAPREVIOUS":
                case "PREVTRACK": virtualKey = 0xB1; return true; // VK_MEDIA_PREV_TRACK
                case "MEDIASTOP": virtualKey = 0xB2; return true; // VK_MEDIA_STOP
                case "MEDIAPLAY":
                case "MEDIAPLAYPAUSE":
                case "PLAYPAUSE": virtualKey = 0xB3; return true; // VK_MEDIA_PLAY_PAUSE
                
                // Browser Keys
                case "BROWSERBACK": virtualKey = 0xA6; return true; // VK_BROWSER_BACK
                case "BROWSERFORWARD": virtualKey = 0xA7; return true; // VK_BROWSER_FORWARD
                case "BROWSERREFRESH": virtualKey = 0xA8; return true; // VK_BROWSER_REFRESH
                case "BROWSERSTOP": virtualKey = 0xA9; return true; // VK_BROWSER_STOP
                case "BROWSERSEARCH": virtualKey = 0xAA; return true; // VK_BROWSER_SEARCH
                case "BROWSERFAVORITES": virtualKey = 0xAB; return true; // VK_BROWSER_FAVORITES
                case "BROWSERHOME": virtualKey = 0xAC; return true; // VK_BROWSER_HOME
                
                // Application Keys
                case "MENU":
                case "APPS": virtualKey = 0x5D; return true; // VK_APPS (context menu)
                case "SLEEP": virtualKey = 0x5F; return true; // VK_SLEEP
                
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Gets a user-friendly description of the virtual key code
        /// </summary>
        /// <param name="virtualKey">The virtual key code</param>
        /// <returns>A string description of the key</returns>
        public static string GetKeyDescription(int virtualKey)
        {
            return virtualKey switch
            {
                >= 0x41 and <= 0x5A => $"Key {(char)virtualKey}", // A-Z
                >= 0x30 and <= 0x39 => $"Key {(char)virtualKey}", // 0-9
                >= 0x70 and <= 0x87 => $"F{virtualKey - 0x6F}", // F1-F24
                0x25 => "Left Arrow",
                0x26 => "Up Arrow", 
                0x27 => "Right Arrow",
                0x28 => "Down Arrow",
                0x20 => "Space",
                0x0D => "Enter",
                0x08 => "Backspace",
                0x09 => "Tab",
                0x1B => "Escape",
                _ => $"VK_{virtualKey:X2}"
            };
        }
        
        /// <summary>
        /// Convenience overload that normalizes input before parsing.
        /// For performance-critical scenarios, use the main overload with pre-normalized input.
        /// </summary>
        /// <param name="keyName">The string name of the key (will be normalized)</param>
        /// <param name="virtualKey">The virtual key code if parsing succeeds</param>
        /// <returns>True if the key was successfully parsed, false otherwise</returns>
        public static bool TryParseVirtualKeyWithNormalization(string keyName, out int virtualKey)
        {
            if (string.IsNullOrWhiteSpace(keyName))
            {
                virtualKey = 0;
                return false;
            }
            
            return TryParseVirtualKey(keyName.ToUpperInvariant().Trim(), out virtualKey);
        }
    }
}
