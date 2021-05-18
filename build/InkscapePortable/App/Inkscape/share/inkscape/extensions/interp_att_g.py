#!/usr/bin/env python
# coding=utf-8
#
# Copyright (C) 2009 Aurelio A. Heckert, aurium (a) gmail dot com
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
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
#
"""
Interpolation of attributes in selected objects or group's children.
"""

import inkex

class InterpAttG(inkex.EffectExtension):
    """
    This effect applies a value for any interpolatable attribute for all
    elements inside the selected group or for all elements in a multiple selection.
    """
    def __init__(self):
        super(InterpAttG, self).__init__()
        self.arg_parser.add_argument(
            "-a", "--att", type=str, dest="att", default="fill",
            help="Attribute to be interpolated.")
        self.arg_parser.add_argument(
            "-o", "--att-other", type=str, dest="att_other",
            help="Other attribute (for a limited UI).")
        self.arg_parser.add_argument(
            "-t", "--att-other-type", type=str, dest="att_other_type",
            help="The other attribute type.")
        self.arg_parser.add_argument(
            "-w", "--att-other-where", type=str, dest="att_other_where",
            help="That is a tag attribute or a style attribute?")
        self.arg_parser.add_argument(
            "-s", "--start-val", type=str, dest="start_val", default="#F00",
            help="Initial interpolation value.")
        self.arg_parser.add_argument(
            "-e", "--end-val", type=str, dest="end_val", default="#00F",
            help="End interpolation value.")
        self.arg_parser.add_argument(
            "-u", "--unit", type=str, dest="unit", default="color",
            help="Values unit.")
        self.arg_parser.add_argument(
            "--zsort", type=inkex.Boolean, dest="zsort", default=True,
            help="use z-order instead of selection order")
        self.arg_parser.add_argument(
            "--tab", type=str, dest="tab",
            help="The selected UI-tab when OK was pressed")

    def get_color_steps(self, total):
        """Get the color value, returning the start color and a single increment step"""
        start_value = inkex.Color(self.options.start_val)
        end_value = inkex.Color(self.options.end_val)

        color_inc = [
            (end_value[v] - start_value[v]) / float(total - 1)
            for v in range(3)]

        return start_value, color_inc

    def get_number_steps(self, total):
        """Get the number value, returning the start float and a single increment step"""
        start_value = self.options.start_val.replace(",", ".")
        end_value = self.options.end_val.replace(",", ".")
        unit = self.options.unit

        if unit != 'none':
            start_value = self.svg.unittouu(start_value + unit)
            end_value = self.svg.unittouu(end_value + unit)

        try:
            start_value = float(start_value)
            end_value = float(end_value)
        except ValueError:
            inkex.errormsg(
                _("Bad values for a number field: {}, {}.".format(start_value, end_value)))
            return 0, 0

        val_inc = (end_value - start_value) / float(total - 1)
        return start_value, val_inc

    def get_elements(self):
        """Returns a list of elements to work on"""
        if not self.svg.selection:
            return []

        if len(self.svg.selection) > 1:
            # multiple selection
            if self.options.zsort:
                return self.svg.selection.paint_order()
            return self.svg.selected

        # must be a group
        node = self.svg.selection.filter(inkex.Group).first()
        return list(node) or []

    def effect(self):
        if self.options.att == 'other':
            if self.options.att_other is not None:
                inte_att = self.options.att_other
            else:
                inkex.errormsg(_("You selected 'Other'. Please enter an attribute to interpolate."))
                return

            inte_att_type = self.options.att_other_type
            where = self.options.att_other_where
        else:
            inte_att = self.options.att
            inte_att_type = 'float'
            if inte_att in ('width', 'height'):
                where = 'tag'
            elif inte_att in ('scale', 'trans-x', 'trans-y'):
                where = 'transform'
            elif inte_att == 'opacity':
                where = 'style'
            elif inte_att in ('fill', 'stroke'):
                inte_att_type = 'color'
                where = 'style'

        collection = self.get_elements()

        if not collection:
            inkex.errormsg(_('There is no selection to interpolate'))
            return False

        if inte_att_type == 'color':
            cur, inc = self.get_color_steps(len(collection))
        else:
            cur, inc = self.get_number_steps(len(collection))

        for node in collection:
            if inte_att_type == 'color':
                val = inkex.Color([int(cur[i]) for i in range(3)])
            elif inte_att_type == 'float':
                val = cur
            elif inte_att_type == 'int':
                val = int(round(cur))
            else:
                raise KeyError("Unknown attr type: {}".format(inte_att_type))

            if where == 'style':
                node.style[inte_att] = str(val)
            elif where == 'transform':
                if inte_att == 'trans-x':
                    node.transform.add_translate(val, 0)
                elif inte_att == 'trans-y':
                    node.transform.add_translate(0, val)
                elif inte_att == 'scale':
                    node.transform.add_scale(val)
            elif where == 'tag':
                node.set(inte_att, str(val))
            else:
                raise KeyError("Unknown update {}".format(where))

            if inte_att_type == 'color':
                cur = [cur[i] + inc[i] for i in range(3)]
            else:
                cur += inc

        return True

if __name__ == '__main__':
    InterpAttG().run()
