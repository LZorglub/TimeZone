<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="text" indent="yes"/>

  <xsl:template match="mapZone">
    <xsl:value-of select="@type"/>
    <xsl:text> </xsl:text>
    <xsl:value-of select="@other"/>
    <xsl:text>&#xd;&#xa;</xsl:text>
  </xsl:template>

  <xsl:template match="/">
    <xsl:apply-templates select="supplementalData/windowsZones/mapTimezones[1]/mapZone[@territory='001']">
      <xsl:sort select="@type" />
    </xsl:apply-templates>
  </xsl:template>
  
  
    <!--<xsl:template match="@* | node()">
        <xsl:copy>
            <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
    </xsl:template>-->
  
</xsl:stylesheet>
