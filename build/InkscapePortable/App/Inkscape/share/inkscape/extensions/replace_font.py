#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2010 Craig Marshall, craig9 [at] gmail.com
#
# This program is free software; you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation; either version 2 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program; if not, write to the Free Software
# Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
#
"""
This script finds all fonts in the current drawing that match the
specified find font, and replaces them with the specified replacement
font.

It can also replace all fonts indiscriminately, and list all fonts
currently being used.
"""
import inkex

text_tags = ['{http://www.w3.org/2000/svg}tspan',
                            '{http://www.w3.org/2000/svg}text',
                            '{http://www.w3.org/2000/svg}flowRoot',
                            '{http://www.w3.org/2000/svg}flowPara',
                            '{http://www.w3.org/2000/svg}flowSpan']
font_attributes = ['font-family', '-inkscape-font-specification']

def set_font(node, new_font, style=None):
    """
    Sets the font attribute in the style attribute of node, using the
    font name stored in new_font. If the style dict is open already,
    it can be passed in, otherwise it will be optned anyway.

    Returns a dirty boolean flag
    """
    dirty = False
    if not style:
        style = get_style(node)
    if style:
        for att in font_attributes:
            if att in style:
                style[att] = new_font
                set_style(node, style)
                dirty = True
    return dirty

def find_replace_font(node, find, replace):
    """
    Searches the relevant font attributes/styles of node for find, and
    replaces them with replace.

    Returns a dirty boolean flag
    """
    dirty = False
    style = get_style(node)
    if style:
        for att in font_attributes:
            if att in style and style[att].strip().lower() == find:
                set_font(node, replace, style)
                dirty = True
    return dirty

def is_styled_text(node):
    """
    Returns true if the tag in question is a "styled" element that
    can hold text.
    """
    return node.tag in text_tags and 'style' in node.attrib

def is_text(node):
    """
    Returns true if the tag in question is an element that
    can hold text.
    """
    return node.tag in text_tags


def get_style(node):
    """
    Sugar coated way to get style dict from a node
    """
    if 'style' in node.attrib:
        return dict(inkex.Style.parse_str(node.attrib['style']))

def set_style(node, style):
    """
    Sugar coated way to set the style dict, for node
    """
    node.attrib['style'] = str(inkex.Style(style))

def get_fonts(node):
    """
    Given a node, returns a list containing all the fonts that
    the node is using.
    """
    fonts = []
    s = get_style(node)
    if not s:
        return fonts
    for a in font_attributes:
        if a in s:
            fonts.append(s[a])
    return fonts

def report_replacements(num):
    """
    Sends a message to the end user showing success of failure
    of the font replacement
    """
    if num == 0:
        inkex.errormsg(_('Couldn\'t find anything using that font, please ensure the spelling and spacing is correct.'))

def report_findings(findings):
    """
    Tells the user which fonts were found, if any
    """
    if len(findings) == 0:
        inkex.errormsg(_("Didn't find any fonts in this document/selection."))
    else:
        if len(findings) == 1:
            inkex.errormsg(_(u"Found the following font only: %s") % findings[0])
        else:
            inkex.errormsg(_(u"Found the following fonts:\n%s") % '\n'.join(findings))

class ReplaceFont(inkex.EffectExtension):
    """
    Replaces all instances of one font with another
    """
    def add_arguments(self, pars):
        pars.add_argument("--fr_find")
        pars.add_argument("--fr_replace")
        pars.add_argument("--r_replace")
        pars.add_argument("--action")
        pars.add_argument("--scope")

    def find_child_text_items(self, node):
        """
        Recursive method for appending all text-type elements
        to self.selected_items
        """
        if is_text(node):
            yield node

        for child in node:
            for textchild in self.find_child_text_items(child):
                yield textchild

    def relevant_items(self, scope):
        """
        Depending on the scope, returns all text elements, or all
        selected text elements including nested children
        """
        items = []
        to_return = []

        selected = self.svg
        if scope == "selection_only":
            selected = self.svg.selected.values()

        for item in selected:
            items.extend(self.find_child_text_items(item))

        if not items:
            return inkex.errormsg(_("There was nothing selected"))

        return items

    def find_replace(self, nodes, find, replace):
        """
        Walks through nodes, replacing fonts as it goes according
        to find and replace
        """
        replacements = 0
        for node in nodes:
            if find_replace_font(node, find, replace):
                replacements += 1
        report_replacements(replacements)

    def replace_all(self, nodes, replace):
        """
        Walks through nodes, setting fonts indiscriminately.
        """
        replacements = 0
        for node in nodes:
            if set_font(node, replace):
                replacements += 1
        report_replacements(replacements)

    def list_all(self, nodes):
        """
        Walks through nodes, building a list of all fonts found, then
        reports to the user with that list
        """
        fonts_found = []
        for node in nodes:
            for f in get_fonts(node):
                if not f in fonts_found:
                    fonts_found.append(f)
        report_findings(sorted(fonts_found))

    def effect(self):
        if not self.options.action:
            return inkex.errormsg("Nothing to do, no action specified.")
        action = self.options.action.strip("\"") # TODO Is this a bug? (Extra " characters)
        scope = self.options.scope

        relevant_items = self.relevant_items(scope)

        if action == "find_replace":
            find = self.options.fr_find
            if find is None or find == "":
                return inkex.errormsg(_("Please enter a search string in the find box."))
            find = find.strip().lower()
            replace = self.options.fr_replace
            if replace is None or replace == "":
                return inkex.errormsg(_("Please enter a replacement font in the replace with box."))
            self.find_replace(relevant_items, find, replace)
        elif action == "replace_all":
            replace = self.options.r_replace
            if replace is None or replace == "":
                return inkex.errormsg(_("Please enter a replacement font in the replace all box."))
            self.replace_all(relevant_items, replace)
        elif action == "list_only":
            self.list_all(relevant_items)

if __name__ == "__main__":
    ReplaceFont().run()
