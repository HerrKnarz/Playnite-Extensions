namespace Playnite;

public static partial class Loc
{

    /// <summary>
    /// Fluent test string
    /// </summary>
    public static string example_string()
    {
        return GetString("example_string");
    }
    /// <summary>
    /// Link Utilities
    /// </summary>
    public static string link_utilities_name()
    {
        return GetString("link_utilities_name");
    }
    /// <summary>
    /// Link from clipboard
    /// </summary>
    public static string action_name_clipboard_links()
    {
        return GetString("action_name_clipboard_links");
    }
    /// <summary>
    /// Steam links converter
    /// </summary>
    public static string action_name_convert_steam_links()
    {
        return GetString("action_name_convert_steam_links");
    }
    /// <summary>
    /// Do after change
    /// </summary>
    public static string action_name_do_after_change()
    {
        return GetString("action_name_do_after_change");
    }
    /// <summary>
    /// Library links
    /// </summary>
    public static string action_name_library_links()
    {
        return GetString("action_name_library_links");
    }
    /// <summary>
    /// Remove duplicate links
    /// </summary>
    public static string action_name_remove_duplicates()
    {
        return GetString("action_name_remove_duplicates");
    }
    /// <summary>
    /// Link from uri
    /// </summary>
    public static string action_name_uri_links()
    {
        return GetString("action_name_uri_links");
    }
    /// <summary>
    /// Website links
    /// </summary>
    public static string action_name_website_links()
    {
        return GetString("action_name_website_links");
    }
    /// <summary>
    /// Link Type
    /// </summary>
    public static string caption_link_type()
    {
        return GetString("caption_link_type");
    }
    /// <summary>
    /// Added links to {$gameCount} games!
    /// </summary>
    public static string dialog_added_links_message(object gameCount)
    {
        return GetString("dialog_added_links_message", ("gameCount", gameCount));
    }
    /// <summary>
    /// Converted Steam links of {$gameCount} games!
    /// </summary>
    public static string dialog_converted_steam_links_message(object gameCount)
    {
        return GetString("dialog_converted_steam_links_message", ("gameCount", gameCount));
    }
    /// <summary>
    /// Please enter a name for the link!
    /// </summary>
    public static string dialog_enter_link_name()
    {
        return GetString("dialog_enter_link_name");
    }
    /// <summary>
    /// Name the link
    /// </summary>
    public static string dialog_name_link_caption()
    {
        return GetString("dialog_name_link_caption");
    }
    /// <summary>
    /// Processed links of {$gameCount} games!
    /// </summary>
    public static string dialog_processed_links_message(object gameCount)
    {
        return GetString("dialog_processed_links_message", ("gameCount", gameCount));
    }
    /// <summary>
    /// Removed duplicate links from {$gameCount} games!
    /// </summary>
    public static string dialog_removed_duplicates_message(object gameCount)
    {
        return GetString("dialog_removed_duplicates_message", ("gameCount", gameCount));
    }
    /// <summary>
    /// A link to {$linkType} already exists. You can replace the existing link or add a new one by entering a new link name.
    /// </summary>
    public static string dialog_replace_link(object linkType)
    {
        return GetString("dialog_replace_link", ("linkType", linkType));
    }
    /// <summary>
    /// Search game
    /// </summary>
    public static string dialog_search_game()
    {
        return GetString("dialog_search_game");
    }
    /// <summary>
    /// Select option
    /// </summary>
    public static string dialog_select_option()
    {
        return GetString("dialog_select_option");
    }
    /// <summary>
    /// link type and URL
    /// </summary>
    public static string enum_duplicate_types_type_url()
    {
        return GetString("enum_duplicate_types_type_url");
    }
    /// <summary>
    /// link type
    /// </summary>
    public static string enum_duplicate_types_type()
    {
        return GetString("enum_duplicate_types_type");
    }
    /// <summary>
    /// URL
    /// </summary>
    public static string enum_duplicate_types_url()
    {
        return GetString("enum_duplicate_types_url");
    }
    /// <summary>
    /// Add library links
    /// </summary>
    public static string menu_add_library_links()
    {
        return GetString("menu_add_library_links");
    }
    /// <summary>
    /// Add link from clipboard
    /// </summary>
    public static string menu_add_link_from_clipboard()
    {
        return GetString("menu_add_link_from_clipboard");
    }
    /// <summary>
    /// All enabled websites
    /// </summary>
    public static string menu_add_link_to_all_enabled_websites()
    {
        return GetString("menu_add_link_to_all_enabled_websites");
    }
    /// <summary>
    /// Convert Steam links to client links
    /// </summary>
    public static string menu_convert_steam_links_to_client()
    {
        return GetString("menu_convert_steam_links_to_client");
    }
    /// <summary>
    /// Convert Steam links to web links
    /// </summary>
    public static string menu_convert_steam_links_to_website()
    {
        return GetString("menu_convert_steam_links_to_website");
    }
    /// <summary>
    /// Remove duplicate links
    /// </summary>
    public static string menu_remove_duplicate_links()
    {
        return GetString("menu_remove_duplicate_links");
    }
    /// <summary>
    /// All missing websites
    /// </summary>
    public static string menu_search_link_to_all_missing_websites()
    {
        return GetString("menu_search_link_to_all_missing_websites");
    }
    /// <summary>
    /// Open browser search on...
    /// </summary>
    public static string menu_search_link_in_browser()
    {
        return GetString("menu_search_link_in_browser");
    }
    /// <summary>
    /// Add Link to...
    /// </summary>
    public static string menu_section_add_link()
    {
        return GetString("menu_section_add_link");
    }
    /// <summary>
    /// Search link to...
    /// </summary>
    public static string menu_section_search_link()
    {
        return GetString("menu_section_search_link");
    }
    /// <summary>
    /// Adding library links...
    /// </summary>
    public static string progress_adding_library_links()
    {
        return GetString("progress_adding_library_links");
    }
    /// <summary>
    /// Adding links...
    /// </summary>
    public static string progress_adding_single_website_links()
    {
        return GetString("progress_adding_single_website_links");
    }
    /// <summary>
    /// Adding links to configured websites...
    /// </summary>
    public static string progress_adding_website_links()
    {
        return GetString("progress_adding_website_links");
    }
    /// <summary>
    /// Converting Steam links...
    /// </summary>
    public static string progress_converting_steam_links()
    {
        return GetString("progress_converting_steam_links");
    }
    /// <summary>
    /// Processing links...
    /// </summary>
    public static string progress_processing_links()
    {
        return GetString("progress_processing_links");
    }
    /// <summary>
    /// Removing duplicate links...
    /// </summary>
    public static string progress_removing_duplicates()
    {
        return GetString("progress_removing_duplicates");
    }
    /// <summary>
    /// Add link
    /// </summary>
    public static string settings_add_link()
    {
        return GetString("settings_add_link");
    }
    /// <summary>
    /// Automatically add links to new games
    /// </summary>
    public static string settings_add_links_to_new_games()
    {
        return GetString("settings_add_links_to_new_games");
    }
    /// <summary>
    /// API-Key
    /// </summary>
    public static string settings_api_key()
    {
        return GetString("settings_api_key");
    }
    /// <summary>
    /// Using a bookmarklet you can add the active website in your browser directly to the selected games in Playnite. To find a suiting link name, you can define patterns for the URL and link title here. The patterns can contain wildcards. A * can be zero or more characters, a ? has to be exactly one character.
    /// </summary>
    public static string settings_bookmarklet_description()
    {
        return GetString("settings_bookmarklet_description");
    }
    /// <summary>
    /// Add
    /// </summary>
    public static string settings_button_add()
    {
        return GetString("settings_button_add");
    }
    /// <summary>
    /// Add defaults
    /// </summary>
    public static string settings_button_add_defaults()
    {
        return GetString("settings_button_add_defaults");
    }
    /// <summary>
    /// Help
    /// </summary>
    public static string settings_button_help()
    {
        return GetString("settings_button_help");
    }
    /// <summary>
    /// Remove
    /// </summary>
    public static string settings_button_remove()
    {
        return GetString("settings_button_remove");
    }
    /// <summary>
    /// Sort
    /// </summary>
    public static string settings_button_sort()
    {
        return GetString("settings_button_sort");
    }
    /// <summary>
    /// Configure link name patterns (For clipboard links only the url pattern will be checked to find a name, since we have no name to begin with.)
    /// </summary>
    public static string settings_configure_link_name_patterns()
    {
        return GetString("settings_configure_link_name_patterns");
    }
    /// <summary>
    /// Configure websites
    /// </summary>
    public static string settings_configure_websites()
    {
        return GetString("settings_configure_websites");
    }
    /// <summary>
    /// Automatically convert steam web links to steam client links on library update.
    /// </summary>
    public static string settings_convert_steam_links_after_change()
    {
        return GetString("settings_convert_steam_links_after_change");
    }
    /// <summary>
    /// Log debug messages
    /// </summary>
    public static string settings_debug_mode()
    {
        return GetString("settings_debug_mode");
    }
    /// <summary>
    /// Remove duplicate links after the game meta data was updated
    /// </summary>
    public static string settings_duplicates_remove_after_change()
    {
        return GetString("settings_duplicates_remove_after_change");
    }
    /// <summary>
    /// Remove duplicate links with same
    /// </summary>
    public static string settings_duplicates_remove_type()
    {
        return GetString("settings_duplicates_remove_type");
    }
    /// <summary>
    /// Link source
    /// </summary>
    public static string settings_link_source()
    {
        return GetString("settings_link_source");
    }
    /// <summary>
    /// Name pattern
    /// </summary>
    public static string settings_name_pattern()
    {
        return GetString("settings_name_pattern");
    }
    /// <summary>
    /// Partial match
    /// </summary>
    public static string settings_partial_match()
    {
        return GetString("settings_partial_match");
    }
    /// <summary>
    /// When checked only one of both patterns has to match.
    /// </summary>
    public static string settings_partial_match_hint()
    {
        return GetString("settings_partial_match_hint");
    }
    /// <summary>
    /// Search link
    /// </summary>
    public static string settings_search_link()
    {
        return GetString("settings_search_link");
    }
    /// <summary>
    /// Show in menu
    /// </summary>
    public static string settings_show_in_menus()
    {
        return GetString("settings_show_in_menus");
    }
    /// <summary>
    /// Bookmarklet/Clipboard
    /// </summary>
    public static string settings_tab_bookmarklet_clipboard()
    {
        return GetString("settings_tab_bookmarklet_clipboard");
    }
    /// <summary>
    /// General settings
    /// </summary>
    public static string settings_tab_general()
    {
        return GetString("settings_tab_general");
    }
    /// <summary>
    /// URL pattern
    /// </summary>
    public static string settings_url_pattern()
    {
        return GetString("settings_url_pattern");
    }
}

public static partial class LocId
{

    /// <summary>
    /// Fluent test string
    /// </summary>
    public const string example_string = "example_string";
    /// <summary>
    /// Link Utilities
    /// </summary>
    public const string link_utilities_name = "link_utilities_name";
    /// <summary>
    /// Link from clipboard
    /// </summary>
    public const string action_name_clipboard_links = "action_name_clipboard_links";
    /// <summary>
    /// Steam links converter
    /// </summary>
    public const string action_name_convert_steam_links = "action_name_convert_steam_links";
    /// <summary>
    /// Do after change
    /// </summary>
    public const string action_name_do_after_change = "action_name_do_after_change";
    /// <summary>
    /// Library links
    /// </summary>
    public const string action_name_library_links = "action_name_library_links";
    /// <summary>
    /// Remove duplicate links
    /// </summary>
    public const string action_name_remove_duplicates = "action_name_remove_duplicates";
    /// <summary>
    /// Link from uri
    /// </summary>
    public const string action_name_uri_links = "action_name_uri_links";
    /// <summary>
    /// Website links
    /// </summary>
    public const string action_name_website_links = "action_name_website_links";
    /// <summary>
    /// Link Type
    /// </summary>
    public const string caption_link_type = "caption_link_type";
    /// <summary>
    /// Added links to {$gameCount} games!
    /// </summary>
    public const string dialog_added_links_message = "dialog_added_links_message";
    /// <summary>
    /// Converted Steam links of {$gameCount} games!
    /// </summary>
    public const string dialog_converted_steam_links_message = "dialog_converted_steam_links_message";
    /// <summary>
    /// Please enter a name for the link!
    /// </summary>
    public const string dialog_enter_link_name = "dialog_enter_link_name";
    /// <summary>
    /// Name the link
    /// </summary>
    public const string dialog_name_link_caption = "dialog_name_link_caption";
    /// <summary>
    /// Processed links of {$gameCount} games!
    /// </summary>
    public const string dialog_processed_links_message = "dialog_processed_links_message";
    /// <summary>
    /// Removed duplicate links from {$gameCount} games!
    /// </summary>
    public const string dialog_removed_duplicates_message = "dialog_removed_duplicates_message";
    /// <summary>
    /// A link to {$linkType} already exists. You can replace the existing link or add a new one by entering a new link name.
    /// </summary>
    public const string dialog_replace_link = "dialog_replace_link";
    /// <summary>
    /// Search game
    /// </summary>
    public const string dialog_search_game = "dialog_search_game";
    /// <summary>
    /// Select option
    /// </summary>
    public const string dialog_select_option = "dialog_select_option";
    /// <summary>
    /// link type and URL
    /// </summary>
    public const string enum_duplicate_types_type_url = "enum_duplicate_types_type_url";
    /// <summary>
    /// link type
    /// </summary>
    public const string enum_duplicate_types_type = "enum_duplicate_types_type";
    /// <summary>
    /// URL
    /// </summary>
    public const string enum_duplicate_types_url = "enum_duplicate_types_url";
    /// <summary>
    /// Add library links
    /// </summary>
    public const string menu_add_library_links = "menu_add_library_links";
    /// <summary>
    /// Add link from clipboard
    /// </summary>
    public const string menu_add_link_from_clipboard = "menu_add_link_from_clipboard";
    /// <summary>
    /// All enabled websites
    /// </summary>
    public const string menu_add_link_to_all_enabled_websites = "menu_add_link_to_all_enabled_websites";
    /// <summary>
    /// Convert Steam links to client links
    /// </summary>
    public const string menu_convert_steam_links_to_client = "menu_convert_steam_links_to_client";
    /// <summary>
    /// Convert Steam links to web links
    /// </summary>
    public const string menu_convert_steam_links_to_website = "menu_convert_steam_links_to_website";
    /// <summary>
    /// Remove duplicate links
    /// </summary>
    public const string menu_remove_duplicate_links = "menu_remove_duplicate_links";
    /// <summary>
    /// All missing websites
    /// </summary>
    public const string menu_search_link_to_all_missing_websites = "menu_search_link_to_all_missing_websites";
    /// <summary>
    /// Open browser search on...
    /// </summary>
    public const string menu_search_link_in_browser = "menu_search_link_in_browser";
    /// <summary>
    /// Add Link to...
    /// </summary>
    public const string menu_section_add_link = "menu_section_add_link";
    /// <summary>
    /// Search link to...
    /// </summary>
    public const string menu_section_search_link = "menu_section_search_link";
    /// <summary>
    /// Adding library links...
    /// </summary>
    public const string progress_adding_library_links = "progress_adding_library_links";
    /// <summary>
    /// Adding links...
    /// </summary>
    public const string progress_adding_single_website_links = "progress_adding_single_website_links";
    /// <summary>
    /// Adding links to configured websites...
    /// </summary>
    public const string progress_adding_website_links = "progress_adding_website_links";
    /// <summary>
    /// Converting Steam links...
    /// </summary>
    public const string progress_converting_steam_links = "progress_converting_steam_links";
    /// <summary>
    /// Processing links...
    /// </summary>
    public const string progress_processing_links = "progress_processing_links";
    /// <summary>
    /// Removing duplicate links...
    /// </summary>
    public const string progress_removing_duplicates = "progress_removing_duplicates";
    /// <summary>
    /// Add link
    /// </summary>
    public const string settings_add_link = "settings_add_link";
    /// <summary>
    /// Automatically add links to new games
    /// </summary>
    public const string settings_add_links_to_new_games = "settings_add_links_to_new_games";
    /// <summary>
    /// API-Key
    /// </summary>
    public const string settings_api_key = "settings_api_key";
    /// <summary>
    /// Using a bookmarklet you can add the active website in your browser directly to the selected games in Playnite. To find a suiting link name, you can define patterns for the URL and link title here. The patterns can contain wildcards. A * can be zero or more characters, a ? has to be exactly one character.
    /// </summary>
    public const string settings_bookmarklet_description = "settings_bookmarklet_description";
    /// <summary>
    /// Add
    /// </summary>
    public const string settings_button_add = "settings_button_add";
    /// <summary>
    /// Add defaults
    /// </summary>
    public const string settings_button_add_defaults = "settings_button_add_defaults";
    /// <summary>
    /// Help
    /// </summary>
    public const string settings_button_help = "settings_button_help";
    /// <summary>
    /// Remove
    /// </summary>
    public const string settings_button_remove = "settings_button_remove";
    /// <summary>
    /// Sort
    /// </summary>
    public const string settings_button_sort = "settings_button_sort";
    /// <summary>
    /// Configure link name patterns (For clipboard links only the url pattern will be checked to find a name, since we have no name to begin with.)
    /// </summary>
    public const string settings_configure_link_name_patterns = "settings_configure_link_name_patterns";
    /// <summary>
    /// Configure websites
    /// </summary>
    public const string settings_configure_websites = "settings_configure_websites";
    /// <summary>
    /// Automatically convert steam web links to steam client links on library update.
    /// </summary>
    public const string settings_convert_steam_links_after_change = "settings_convert_steam_links_after_change";
    /// <summary>
    /// Log debug messages
    /// </summary>
    public const string settings_debug_mode = "settings_debug_mode";
    /// <summary>
    /// Remove duplicate links after the game meta data was updated
    /// </summary>
    public const string settings_duplicates_remove_after_change = "settings_duplicates_remove_after_change";
    /// <summary>
    /// Remove duplicate links with same
    /// </summary>
    public const string settings_duplicates_remove_type = "settings_duplicates_remove_type";
    /// <summary>
    /// Link source
    /// </summary>
    public const string settings_link_source = "settings_link_source";
    /// <summary>
    /// Name pattern
    /// </summary>
    public const string settings_name_pattern = "settings_name_pattern";
    /// <summary>
    /// Partial match
    /// </summary>
    public const string settings_partial_match = "settings_partial_match";
    /// <summary>
    /// When checked only one of both patterns has to match.
    /// </summary>
    public const string settings_partial_match_hint = "settings_partial_match_hint";
    /// <summary>
    /// Search link
    /// </summary>
    public const string settings_search_link = "settings_search_link";
    /// <summary>
    /// Show in menu
    /// </summary>
    public const string settings_show_in_menus = "settings_show_in_menus";
    /// <summary>
    /// Bookmarklet/Clipboard
    /// </summary>
    public const string settings_tab_bookmarklet_clipboard = "settings_tab_bookmarklet_clipboard";
    /// <summary>
    /// General settings
    /// </summary>
    public const string settings_tab_general = "settings_tab_general";
    /// <summary>
    /// URL pattern
    /// </summary>
    public const string settings_url_pattern = "settings_url_pattern";
}
