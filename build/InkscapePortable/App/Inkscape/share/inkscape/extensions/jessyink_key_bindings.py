#!/usr/bin/env python
#
# Copyright 2008, 2009 Hannes Hochreiner
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see http://www.gnu.org/licenses/.
#
"""Effect to add key bindings to jessyInk slide show"""

import inkex
from inkex import Group, Script
from jessyink_install import JessyInkMixin

KEY_CODES = ('LEFT', 'RIGHT', 'DOWN', 'UP', 'HOME', 'END',
             'ENTER', 'SPACE', 'PAGE_UP', 'PAGE_DOWN', 'ESCAPE')

class KeyBindings(JessyInkMixin, inkex.EffectExtension):
    """Add key bindings to slide show"""
    modes = ('slide', 'index', 'drawing')

    def set_options(self, namespace, opt_str, value):
        """Sort through all the options and combine them"""
        slot, action = opt_str.split('_', 1)
        keycodes = getattr(namespace, slot + "KeyCodes", {})
        charcodes = getattr(namespace, slot + "CharCodes", {})
        if value:
            for val in value.split(","):
                val = val.strip()
                if val in KEY_CODES:
                    keycodes[val + "_KEY"] = self.actions[slot][action]
                elif len(val) == 1:
                    charcodes[val] = self.actions[slot][action]
        setattr(namespace, slot + "KeyCodes", keycodes)
        setattr(namespace, slot + "CharCodes", charcodes)

    actions = {
        'slide': {
            "export": "slideUpdateExportLayer();",
            "addSlide": "slideAddSlide(activeSlide);",
            "resetTimer": "slideResetTimer();",
            "setDuration": "slideQueryDuration();",
            "backWithEffects": "dispatchEffects(-1);",
            "nextWithEffects": "dispatchEffects(1);",
            "backWithoutEffects": "skipEffects(-1);",
            "nextWithoutEffects": "skipEffects(1);",
            "switchToIndexMode": "toggleSlideIndex();",
            "switchToDrawingMode": "slideSwitchToDrawingMode();",
            "toggleProgressBar": "slideToggleProgressBarVisibility();",
            "firstSlide": "slideSetActiveSlide(0);",
            "lastSlide": "slideSetActiveSlide(slides.length - 1);",
        },
        'drawing': {
            "undo": "drawingUndo();",
            "switchToSlideMode": "drawingSwitchToSlideMode();",
            "pathWidthDefault": "drawingResetPathWidth();",
            "pathWidth1": "drawingSetPathWidth(1.0);",
            "pathWidth3": "drawingSetPathWidth(3.0);",
            "pathWidth5": "drawingSetPathWidth(5.0);",
            "pathWidth7": "drawingSetPathWidth(7.0);",
            "pathWidth9": "drawingSetPathWidth(9.0);",
            "pathColourBlue": "drawingSetPathColour(\"blue\");",
            "pathColourCyan": "drawingSetPathColour(\"cyan\");",
            "pathColourGreen": "drawingSetPathColour(\"green\");",
            "pathColourBlack": "drawingSetPathColour(\"black\");",
            "pathColourMagenta": "drawingSetPathColour(\"magenta\");",
            "pathColourOrange": "drawingSetPathColour(\"orange\");",
            "pathColourRed": "drawingSetPathColour(\"red\");",
            "pathColourWhite": "drawingSetPathColour(\"white\");",
            "pathColourYellow": "drawingSetPathColour(\"yellow\");",
        },
        'index': {
            "selectSlideToLeft": "indexSetPageSlide(activeSlide - 1);",
            "selectSlideToRight": "indexSetPageSlide(activeSlide + 1);",
            "selectSlideAbove": "indexSetPageSlide(activeSlide - INDEX_COLUMNS);",
            "selectSlideBelow": "indexSetPageSlide(activeSlide + INDEX_COLUMNS);",
            "previousPage": "indexSetPageSlide(activeSlide - INDEX_COLUMNS * INDEX_COLUMNS);",
            "nextPage": "indexSetPageSlide(activeSlide + INDEX_COLUMNS * INDEX_COLUMNS);",
            "firstSlide": "indexSetPageSlide(0);",
            "lastSlide": "indexSetPageSlide(slides.length - 1);",
            "switchToSlideMode": "toggleSlideIndex();",
            "decreaseNumberOfColumns": "indexDecreaseNumberOfColumns();",
            "increaseNumberOfColumns": "indexIncreaseNumberOfColumns();",
            "setNumberOfColumnsToDefault": "indexResetNumberOfColumns();",
        }
    }

    def add_arguments(self, pars):
        pars.add_argument('--tab')
        for slot, actions in self.actions.items():
            for action in actions:
                pars.add_argument('--{slot}_{action}'.format(slot=slot, action=action))

    def effect(self):
        self.is_installed()

        for name in list(self.options.__dict__):
            if '_' in name:
                self.set_options(self.options, name, self.options.__dict__.pop(name))

        # Remove old master slide property
        for node in self.svg.xpath("//svg:g[@jessyink:customKeyBindings='customKeyBindings']"):
            node.delete()

        # Set custom key bindings.
        node_text = """function getCustomKeyBindingsSub()
{
    var keyDict = new Object();
    keyDict[SLIDE_MODE] = new Object();
    keyDict[INDEX_MODE] = new Object();
    keyDict[DRAWING_MODE] = new Object();
"""

        for key, value in self.options.slideKeyCodes.items():
            node_text += "    keyDict[SLIDE_MODE][{key}] = function() {{ {value} }};\n".format(**locals())

        for key, value in self.options.drawingKeyCodes.items():
            node_text += "    keyDict[DRAWING_MODE][{key}] = function() {{ {value} }};\n".format(**locals())

        for key, value in self.options.indexKeyCodes.items():
            node_text += "    keyDict[INDEX_MODE][{key}] = function() {{ {value} }};\n".format(**locals())

        # Set custom char bindings.
        node_text += """    return keyDict;
}

function getCustomCharBindingsSub()
{
    var charDict = new Object();
    charDict[SLIDE_MODE] = new Object();
    charDict[INDEX_MODE] = new Object();
    charDict[DRAWING_MODE] = new Object();
"""

        for key, value in self.options.slideCharCodes.items():
            node_text += '    charDict[SLIDE_MODE]["{key}"] = function() {{ {value} }};\n'.format(**locals())

        for key, value in self.options.drawingCharCodes.items():
            node_text += '    charDict[DRAWING_MODE]["{key}"] = function() {{ {value} }};\n'.format(**locals())

        for key, value in self.options.indexCharCodes.items():
            node_text += '    charDict[INDEX_MODE]["{key}"] = function() {{ {value} }};\n'.format(**locals())

        node_text += "    return charDict;" + "\n"
        node_text += "}" + "\n"

        # Create new script node
        group = self.svg.add(Group())
        script = group.add(Script())
        script.text = node_text
        group.set("jessyink:customKeyBindings", "customKeyBindings")
        group.set("onload", "this.getCustomCharBindings = function() { "\
            "return getCustomCharBindingsSub(); }; "\
            "this.getCustomKeyBindings = function() { return getCustomKeyBindingsSub(); };")


if __name__ == '__main__':
    KeyBindings().run()
