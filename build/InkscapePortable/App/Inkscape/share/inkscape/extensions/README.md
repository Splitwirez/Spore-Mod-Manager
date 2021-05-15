# Inkscape Extensions

This folder contains the stock Inkscape extensions, i.e. the scripts that
implement some commands that you can use from within Inkscape. Most of
these commands are in the Extensions menu.

## Installation

These scripts should be installed with an Inkscape package already (if you have 
installed Inkscape). For packagers or people testing newer releases, you can 
install the files into /usr/share/inkscape/extensions or 
~/.config/inkscape/extensions .

## Testing

These extensions are designed to have good test coverage as well as python 2.7 
and python 3.6 support.  
Testing can be run using the setup.py command or pytest directly:

    ./setup.py test
    python2 -m pytest
    python3 -m pytest

The latest coverage report for master branch can be found at
https://inkscape.gitlab.io/extensions/coverage/.

## Testing Options

Tests can be run with these options that are provided as environment variables:

    FAIL_ON_DEPRECATION=1 - Will instantly fail any use of deprecated APIs
    EXPORT_COMPARE=1 - Generate output files from comparisions. This is useful for manually checking the output as well as updating the comparison data.
    NO_MOCK_COMMANDS=1 - Instead of using the mock data, actually call commands. This will also generate the msg files similar to export compare.
    INKSCAPE_COMMAND=/other/inkscape - Use a different Inkscape (for example development version) while running commands. Works outside of tests too.
    XML_DIFF=1 - Attempt to output an XML diff file, this can be useful for debugging to see differences in context.
    DEBUG_KEY=1 - Export mock file keys for debugging. This is a highly specialised option for debuging key generation.

## Extension description

Each *.inx file describes an extension, listing its name, purpose,
prerequisites, location within the menu, etc. These files are read by
Inkscape on launch. Other files are the scripts themselves (Perl,
Python, and Ruby are supported, as well as shell scripts).

## Development

Development of both the core inkex modules, tests and each of the extensions
contained within the core inkscape extensions repository should follow these
basic rules of quality assurance:

 * Use pylint to ensure code is written consistantly
 * Have tests so that each line of an extension is covered in the coverage report
 * Not cross streams between extensions, so your extension should import from
   a module and not from another extension.
 * Use translations on text for display to users using get text.
 * Should not require external programs to work (with some exceptions)

Also join the community on chat.inkscape.org channel #inkscape_extensions with any
doubts or problems.

## Building Docs

You may wish to compile to docs for use outside of the Inkscape docs, this can 
be done with these commands:

    sphinx-apidoc -F -o source inkex
    ./setup.py build_sphinx -s source
    firefox ./build/sphinx/html/inkex.html

All documentation should be included INSIDE of each python module.

The latest documentation for master branch can be found at
https://inkscape.gitlab.io/extensions/documentation/.

## License Requirements

Only include extensions here which are GPL-compatible.  This includes
Apache-2, MPL 1.1, certain Creative Commons licenses, and more.  See
https://www.gnu.org/licenses/license-list.html for guidance.
