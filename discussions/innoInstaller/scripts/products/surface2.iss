[CustomMessages]
surface2_title=Microsoft Surface 2 Runtime
surface2_size=2.7 MB

[Code]
	
const
	surface2_reg = 'SOFTWARE\Microsoft\Surface\v2.0\Runtime';
	surface2_url = 'http://www.microsoft.com/downloads/info.aspx?na=41&srcfamilyid=61130758-0275-448f-8a25-bd09772113ab&srcdisplaylang=en&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2f4%2f4%2fE%2f44E7A6B8-7F51-4B1B-A5FF-B02E4C8F2BA7%2fSurfaceRuntime.msi';
	
function Surface2Installed(): boolean;
begin	
	Result := RegKeyExists(HKLM, surface2_reg);
end;
 
procedure Surface2();
begin
	if not Surface2Installed() then
		AddProduct('SurfaceRuntime.msi',
			'',
			CustomMessage('surface2_title'),
			CustomMessage('surface2_size'),
			surface2_url,
			false, false);
end;