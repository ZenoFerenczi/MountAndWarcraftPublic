<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output omit-xml-declaration="no" indent="yes" />

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="/EquipmentRosters/EquipmentRoster/EquipmentSet/Equipment[
      @id = 'Item.dreadlordclaw' or
      @id = 'Item.succubus_dagger' or
      @id = 'Item.warden_glaive'
    ]" />
</xsl:stylesheet>
