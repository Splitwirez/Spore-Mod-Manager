#!/usr/bin/env python
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

import inkex
from inkex.localization import inkex_gettext as _

import inkwebeffect

class TransmitAttribute(inkwebeffect.InkWebEffect):
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument("--att", default="fill", help="Attribute to transmitted.")
        pars.add_argument("--when", default="onclick", help="When it must to transmit?")
        pars.add_argument("--from-and-to", dest="from_and_to", default="g-to-one")
        pars.add_argument("--compatibility", default="append",
                          help="Compatibility with previews code to this event.")

    def effect(self):
        self.ensureInkWebSupport()

        if len(self.options.ids) < 2:
            raise inkex.AbortExtension(_("You must select at least two elements."))

        # All set the last else The first set all
        split = -1 if self.options.from_and_to == "g-to-one" else 1
        el_from = list(self.svg.selection.values())[:split]
        id_to = list(self.svg.selection.ids)[split:]

        ev_code = "InkWeb.transmitAtt({{from:this, to:['{}'], att:'{}'}})".format("','".join(id_to), self.options.att)
        for elem in el_from:
            prev_ev_code = elem.get(self.options.when, "")
            if self.options.compatibility == 'append':
                el_ev_code = prev_ev_code + ";\n" + ev_code
            if self.options.compatibility == 'prepend':
                el_ev_code = ev_code + ";\n" + prev_ev_code
            elem.set(self.options.when, el_ev_code)

if __name__ == '__main__':
    TransmitAttribute().run()
