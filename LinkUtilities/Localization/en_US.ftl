## terms used in other translations

-link_utilities_name = Link Utilities
-link_type           = Link Type
-url                 = URL

## messages

link_utilities_name = { -link_utilities_name }

action_name_clipboard_links     = Link from clipboard
action_name_convert_steam_links = Steam links converter
action_name_clean_up_links      = Clean up links
action_name_library_links       = Library links
action_name_remove_duplicates   = Remove duplicate links
action_name_remove_links        = Remove unwanted links
action_name_uri_links           = Link from uri
action_name_website_links       = Website links

caption_link_type = { -link_type }

dialog_added_links_message           = Added links to {$gameCount} games!
dialog_converted_steam_links_message = Converted Steam links of {$gameCount} games!
dialog_enter_link_name               = Please enter a name for the link!
dialog_name_link_caption             = Name the link
dialog_processed_links_message       = Processed links of {$gameCount} games!
dialog_removed_duplicates_message    = Removed duplicate links from {$gameCount} games!
dialog_removed_links_message         = Removed unwanted links from {$gameCount} games!
dialog_replace_link                  = A link to {$linkType} already exists. You can replace the existing link or add a new one by entering a new link name.
dialog_search_game                   = Search game
dialog_select_option                 = Select option

enum_duplicate_types_type_url = { -link_type } and { -url }
enum_duplicate_types_type     = { -link_type }
enum_duplicate_types_url      = { -url }

menu_add_library_links                   = Add library links
menu_add_link_from_clipboard             = Add link from clipboard
menu_add_link_to_all_enabled_websites    = All enabled websites
menu_clean_up_links                      = Clean up links (convert, remove)
menu_convert_steam_links_to_client       = Convert Steam links to client links
menu_convert_steam_links_to_website      = Convert Steam links to web links
menu_remove_duplicate_links              = Remove duplicate links
menu_remove_unwanted_links               = Remove unwanted links
menu_search_link_to_all_missing_websites = All missing websites
menu_search_link_in_browser              = Open browser search on...
menu_section_add_link                    = Add Link to...
menu_section_search_link                 = Search link to...

progress_adding_library_links        = Adding library links...
progress_adding_single_website_links = Adding links...
progress_adding_website_links        = Adding links to configured websites...
progress_converting_steam_links      = Converting Steam links...
progress_processing_links            = Processing links...
progress_removing_duplicates         = Removing duplicate links...
progress_removing_links              = Removing unwanted links...

settings_add_link                          = Add link
settings_add_links_to_new_games            = Automatically add links to new games
settings_api_key                           = API-Key
settings_button_add                        = Add
settings_button_add_defaults               = Add defaults
settings_button_help                       = Help
settings_button_remove                     = Remove
settings_button_sort                       = Sort
settings_configure_websites                = Configure websites
settings_configure_websites_description    = In this list you find all websites supported for automatic link addition. You can configure if they will be processed automatically when adding or searching links and if they show up in the menus. Some websites required an API key to work, that you can add for those links as well. For further info just click the help button.
settings_convert_steam_links_after_change  = Automatically convert steam web links to steam client links on library update.
settings_debug_mode                        = Log debug messages
settings_duplicates_remove_after_change    = Remove duplicate links after the game meta data was updated.
settings_duplicates_remove_type            = Remove duplicate links with same
settings_is_regex                          = Regex
settings_link_name                         = Link Name
settings_link_name_description             = Name of the pattern. Will be used as the actual link name and type if no link type is specified.
settings_link_type_description             = Type the link will be assigned to when the patterns match. If no type is specified the link name will be used.
settings_name_pattern                      = Name Pattern
settings_name_pattern_description          = Pattern to match the link name. Tick the regex check box to treat the pattern as a regular expression. If left empty, the name will be ignored when matching.
settings_partial_match                     = Partial Match
settings_partial_match_description         = When checked only one of both patterns has to match.
settings_remove_link_type_description      = Type of the link to remove. If no type is specified, only the URL pattern will be matched.
settings_remove_unwanted_after_change      = Remove unwanted links after the game meta data was updated.
settings_search_link                       = Search Link
settings_show_in_menus                     = Show in menu
settings_tab_assign_link_types             = Assign link types
settings_tab_assign_link_types_description = To better organize your links you can set up patterns here to assign specific link types to your links. For example have links from wikipedia.com assigned to the link type "Wikipedia" or all links with a name like "official" assigned to a link type "Official Website". These patterns work for automatically or manually assigned links after metadata updates as well as links added via the bookmarklet or the clipboard (only the url will be matched here, since we don't have a name in the clipboard). The patterns can contain wildcards. A * can be zero or more characters, a ? has to be exactly one character. Alternatively you can use the "Regex" checkbox and treat the patterns as regular expressions.
settings_tab_general                       = General Settings
settings_tab_remove_links                  = Remove Links
settings_tab_remove_links_description      = Often metadata providers automatically add links to some websites you don't want. Here you can define patterns for those links you want to remove. The url patterns can contain wildcards. A * can be zero or more characters, a ? has to be exactly one character. Alternatively you can use the "Regex" checkbox and treat the pattern as regular expressions. You need to specify an URL pattern, link type or both to match a link. If only one of them is specified, the other will be ignored when matching.
settings_url_pattern                       = URL Pattern
settings_url_pattern_description           = Pattern to match the link url. Tick the regex check box to treat the pattern as a regular expression. If left empty, the url will be ignored when matching.
