Cell styles
***********

Cells can be styled, their background, border, font of the content and many other options.

Fluent vs properties
####################
The style can be set through properties or through fluent API. Both style produce same result.

.. code-block:: csharp

   // Set style through property
   ws.Cell("A1").Style.Font.FontSize = 20;
   ws.Cell("A1").Style.Font.FontName = "Arial";

   // Set style using fluent API
   ws.Cell("A1").Style
       .Font.SetFontSize(20)
       .Font.SetFontName("Arial");

Font
####

.. code-block:: csharp

   ws.Cell("A1").Style
       .Font.SetFontSize(20)
       .Font.SetFontName("Arial");

Background color
-------------------

.. code-block:: csharp

	ws.Cell("A1").Style
		.Fill.SetBackgroundColor(XLColor.Red);

-----------
Cell border
-----------
You can set a border of a cell.

.. code-block:: csharp

   // Default color is black
   ws.Cell("B2").Style
       .Border.SetTopBorder(XLBorderStyleValues.Medium)
       .Border.SetRightBorder(XLBorderStyleValues.Medium)
       .Border.SetBottomBorder(XLBorderStyleValues.Medium)
       .Border.SetLeftBorder(XLBorderStyleValues.Medium);

Number format
#############

Excel has an ability to format a value in a cell through a built-in number
format or through a custom number format code.

.. image:: img/cell-format-number-format-dialog.png
  :alt: Number format code in a cell format dialog
  :width: 74 %
.. image:: img/cell-format-number-format-ribbon.png
  :alt: Number format options in a ribbon
  :width: 23 %

ClosedXML can set format of a cell or a range through ``IXLStyle.NumberFormat``
property. The property can be found on interfaces of ranges, e.g. ``IXLCell``,
``IXLCells`` or ``IXLRangeBase``. The built-in style is set by setting
a (in theory well known) number format id. Custom style is set by setting
a number format code.

Built-in number format
----------------------

To use built-in number format, first determine a number format id.
ClosedXML contains a subset of common format ids (0..49) in a static class
``XLPredefinedFormat`` or you can just pick an integer from OpenXML SDK
documentation `NumberingFormat Class <https://learn.microsoft.com/en-us/dotnet/api/documentformat.openxml.spreadsheet.numberingformat>`_.

.. warning::
   The built-in formats are identified in an excel only as an id, but actual
   formatting for for the built-in formats differs depending on a locale
   of the Excel.

   As an example, built-in format 20 (``XLPredefinedFormat.DateTime.Hour24Minutes``)
   uses formatting string *h:mm* for en-US locale, but *hh:mm* in en-GB locale.
   That means, if the same workbook is opened in US Excel, Excel will display *8:25*,
   but if the same workbook is opened in UK Excel, it will display *08:25*.

   *en-US* and *en-GB* both use english, so the built-in format can differ even
   for the same language in different territories, not just different languages.

   Some locales also have more built-in formats than others, e.g *th-th* locale
   has some ids that are over 80.

You can set a built-in number format by setting a ``NumberFormatId`` property like this:
``ws.Cell("A1").Style.NumberFormat.NumberFormatId = (int)XLPredefinedFormat.Number.PercentInteger``

ClosedXML doesn't contain all possible values for all locales. You either
have to look through specification (ECMA-376 18.8.30), but it is not
exhaustive.

The other option just change the value in the saves excel (`/xl/styles.xml`,
tag `xf`, attribute `numFmtId`) and observe results in different locales.
On Windows, you can change locale through Settings dialogue:
 * Open *Settings*
 * Go to *Time & language* on the left panel
 * Open *Language & region *in the right panel
 * Change *Regional format*
 * Restart Excel

Custom number format
--------------------

Number format can be more tailored through a custom format code. All formats,
that are not built-in, use custom format code, e.g. if you specify in the format
in Excel dialogue to display value as a number with 5 decimal digits and to use 1000
serparator, Excel will store the format in the file as ``#,##0.00000``.

.. image:: img/cell-format-number-custom-format-code-example.png

The format code can be rather complicated, but is well described in official
documentation `Number format codes <https://support.microsoft.com/en-us/office/number-format-codes-5026bbd6-04bc-48cd-bf33-80f18b4eae68>`_
or the chapter 18.8.31 of the ECMA-376.

.. code-block:: csharp

   using var wb = new XLWorkbook();
   var ws = wb.AddWorksheet();
   var format = "#,##0.0; (#,##0.000)";

   ws.Cell("A1").Value = 10;
   ws.Cell("A1").Style.NumberFormat.Format = format;

   ws.Cell("A2").Value = -10;
   ws.Cell("A2").Style.NumberFormat.Format = format;
   wb.SaveAs("cell-format-custom-format.xlsx");

.. image:: img/cell-format-number-custom-format-code.png
  :alt: The output of the code sample

Neither built-in number format nor custom number format change the value of
a cell. The value stays the same, only presentation of the value changes.
It is possible to get formatted string ``IXLCell.GetFormattedString()``
method.

.. warning::
   The format code in the xlsx file must be in some cases escaped, but
   ClosedXML doesn't do that at the moment. If you encounter a problem
   that value isn't formatted correcty, set the format in an Excel,
   save the xlsx file, change the *xlsx* extension to *zip* and use
   correctly escaped format code from `/xl/styles.xml` file in the zip.
