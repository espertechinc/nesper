<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" />

  <xsl:template match="*">
    <xsl:element name="{name()}">
      <xsl:apply-templates select="@*" />
      <xsl:apply-templates select="node()" />
    </xsl:element>
  </xsl:template>

  <xsl:template match="@*">
    <xsl:attribute name="{name()}">
      <xsl:copy />
      <!-- <xsl:node-of select="."/> -->
    </xsl:attribute>
  </xsl:template>

  <xsl:template match="/*">
    <xsl:apply-templates select="node()" />
  </xsl:template>

  <xsl:template match="/">
    <xsl:copy>
      <xsl:apply-templates />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="comment() | processing-instruction() | text()">
    <xsl:copy />
  </xsl:template>
</xsl:stylesheet>