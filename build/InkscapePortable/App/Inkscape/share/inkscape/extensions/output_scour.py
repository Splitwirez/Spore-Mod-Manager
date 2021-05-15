#!/usr/bin/env python
"""
Run the scour module on the svg output.
"""

from distutils.version import StrictVersion

import inkex

try:
    import scour
    from scour.scour import scourString
except ImportError:
    raise inkex.DependencyError("""Failed to import module 'scour'.
Please make sure it is installed (e.g. using 'pip install scour'
  or 'sudo apt-get install python-scour') and try again.
""")


class ScourInkscape(inkex.OutputExtension):
    """Scour Inkscape Extension"""

        # Scour options
    def add_arguments(self, pars):
        pars.add_argument("--tab")
        pars.add_argument("--simplify-colors", type=inkex.Boolean, dest="simple_colors")
        pars.add_argument("--style-to-xml", type=inkex.Boolean)
        pars.add_argument("--group-collapsing", type=inkex.Boolean, dest="group_collapse")
        pars.add_argument("--create-groups", type=inkex.Boolean, dest="group_create")
        pars.add_argument("--enable-id-stripping", type=inkex.Boolean, dest="strip_ids")
        pars.add_argument("--shorten-ids", type=inkex.Boolean)
        pars.add_argument("--shorten-ids-prefix")
        pars.add_argument("--embed-rasters", type=inkex.Boolean)
        pars.add_argument("--keep-unreferenced-defs", type=inkex.Boolean, dest="keep_defs")
        pars.add_argument("--keep-editor-data", type=inkex.Boolean)
        pars.add_argument("--remove-metadata", type=inkex.Boolean)
        pars.add_argument("--strip-xml-prolog", type=inkex.Boolean)
        pars.add_argument("--set-precision", type=int, dest="digits")
        pars.add_argument("--indent", dest="indent_type")
        pars.add_argument("--nindent", type=int, dest="indent_depth")
        pars.add_argument("--line-breaks", type=inkex.Boolean, dest="newlines")
        pars.add_argument("--strip-xml-space", type=inkex.Boolean, dest="strip_xml_space_attribute")
        pars.add_argument("--protect-ids-noninkscape", type=inkex.Boolean)
        pars.add_argument("--protect-ids-list")
        pars.add_argument("--protect-ids-prefix")
        pars.add_argument("--enable-viewboxing", type=inkex.Boolean)
        pars.add_argument("--enable-comment-stripping", type=inkex.Boolean, dest="strip_comments")
        pars.add_argument("--renderer-workaround", type=inkex.Boolean)

        # options for internal use of the extension
        pars.add_argument("--scour-version")
        pars.add_argument("--scour-version-warn-old", type=inkex.Boolean)

    def save(self, stream):
        # version check if enabled in options
        if self.options.scour_version_warn_old:
            scour_version = scour.__version__
            scour_version_min = self.options.scour_version
            if StrictVersion(scour_version) < StrictVersion(scour_version_min):
                raise inkex.AbortExtension("""
The extension 'Optimized SVG Output' is designed for Scour {scour_version_min} or later but you're
 using the older version Scour {scour_version}.

Note: You can permanently disable this message on the 'About' tab of the extension window.""".format(**locals()))
        del self.options.scour_version
        del self.options.scour_version_warn_old

        # do the scouring
        stream.write(scourString(self.svg.tostring(), self.options).encode('utf8'))

if __name__ == '__main__':
    ScourInkscape().run()
