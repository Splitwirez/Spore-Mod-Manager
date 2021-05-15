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
"""Effect to print out jessyInk summary"""

from collections import defaultdict
import inkex

from jessyink_install import JessyInkMixin, _

class Summary(JessyInkMixin, inkex.EffectExtension):
    """Print of jessyInk summary"""
    def add_arguments(self, pars):
        pars.add_argument('--tab')

    def effect(self):
        self.is_installed()

        # Find the script node, if present
        for node in self.svg.xpath("//svg:script[@id='JessyInk']"):
            version = node.get("jessyink:version")
            if version:
                self.msg(_("JessyInk script version {} installed.".format(version)))
            else:
                self.msg(_("JessyInk script installed."))

        slides = []
        master_slide = None

        for node in self.svg.descendants().get(inkex.Layer).values():
            if node.get("jessyink:master_slide"):
                master_slide = node
            else:
                slides.append(node)

        if master_slide is not None:
            self.msg(_("\nMaster slide:"))
            self.describe_node(master_slide, "\t",\
                ["<the number of the slide>", len(slides), "<the title of the slide>"])

        for i, slide in enumerate(slides):
            self.msg(_("\nSlide {0!s}:").format(i+1))
            self.describe_node(slide, "\t", [i + 1, len(slides), slide.label])

    def describe_node(self, node, prefix, dat):
        """Standard print out formatter"""

        self.msg(_("{prefix}Layer name: {node.label}".format(**locals())))
        self.describe_transition(node, prefix, "In")
        self.describe_transition(node, prefix, "Out")
        self.describe_autotext(node, prefix, dat)
        self.describe_effects(node, prefix)

    def describe_transition(self, node, prefix, transition):
        """Display information about transitions."""
        trans = inkex.Style(node.get("jessyink:transition" + transition))
        if trans:
            name = trans["name"]
            if name != "appear" and "length" in trans:
                length = int(trans["length"] / 1000.0)
                self.msg(_("{prefix}Transition {transition}: {name} ({length!s} s)".format(**locals())))
            else:
                self.msg(_("{prefix}Transition {transition}: {name}".format(**locals())))

    def describe_autotext(self, node, prefix, dat):
        """Display information about auto-texts."""
        auto_texts = {"slide_num" : dat[0], "num" : dat[1], "title" : dat[2]}
        for x, child in enumerate(node.xpath(".//*[@jessyink:autoText]")):
            if not x:
                self.msg(_("\n{}Auto-texts:".format(prefix)))

            pid = child.getparent().get("id")
            val = auto_texts[child.get('jessyink:autoText')]
            self.msg(_(
                '{prefix}\t"{child.text}" (object id "{pid}") will be replaced by "{val}".'.format(**locals())))

    def describe_effects(self, node, prefix):
        """Display information about effects."""
        effects = sorted(self.collect_effects(node), key=lambda val: val[0])
        for x, (enum, effect) in enumerate(effects):
            ret = ""

            order = effect[0]["order"]
            if not x:
                ret += _("\n{prefix}Initial effect (order number {order}):".format(**locals()))
            else:
                ret += _("\n{prefix}Effect {enum!s} (order number {order}):".format(**locals()))

            for item in effect:
                eid = item["id"]
                if item["type"] == "view":
                    ret += _("{prefix}\tView will be set according to object \"{eid}\"".format(**locals()))
                else:
                    ret += _("{prefix}\tObject \"{eid}\"".format(**locals()))

                    if item["direction"] == "in":
                        ret += _(" will appear")
                    elif item["direction"] == "out":
                        ret += _(" will disappear")

                if item["name"] != "appear":
                    ret += _(" using effect \"{0}\"").format(item["name"])

                if "length" in item:
                    ret += _(" in {0!s} s").format(int(item["length"]) / 1000.0)

            self.msg(ret + ".\n")

    @staticmethod
    def collect_effects(node):
        """Collect information about effects."""
        effects = defaultdict(list)
        for child in node.xpath(".//*[@jessyink:effectIn]"):
            effect_data = inkex.Style(child.get('jessyink:effectIn'))
            effect_data["direction"] = "in"
            effect_data["id"] = child.get("id")
            effect_data["type"] = "effect"
            effects[effect_data["order"]].append(effect_data)

        for child in node.xpath(".//*[@jessyink:effectOut]"):
            effect_data = inkex.Style(child.get('jessyink:effectOut'))
            effect_data["direction"] = "out"
            effect_data["id"] = child.get("id")
            effect_data["type"] = "effect"
            effects[effect_data["order"]].append(effect_data)

        for child in node.xpath(".//*[@jessyink:view]"):
            effect_data = inkex.Style(child.get('jessyink:view'))
            effect_data["id"] = child.get("id")
            effect_data["type"] = "view"
            effects[effect_data["order"]].append(effect_data)
        return effects

if __name__ == '__main__':
    Summary().run()
