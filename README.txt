��� ������ � ���������� ���������� ������� ��������� ��� ������.

��� ����� ������������� �� ����� ������� ��� ������� ������� �������������� � ��������� ������ ������:

EXEC sp_addlinkedserver
@server='Azure',
@srvproduct='',    
@provider='sqlncli',
@datasrc='tcp:stasdonserver.database.windows.net,1433',
@location='',
@provstr='',
@catalog='YandexModels'

EXEC sp_addlinkedsrvlogin 'Azure', 'FALSE', NULL, 'ReadOnlyLogin', 'ReadOnly1';

����� ����� ������� � ���������� GoodsReviews_Setup\Debug, ��������� ����� ����������� Windows GoodsReviews_Setup.msi � �������� ��� �����������.
�����!!!! ��� �������� ����� ��������� ���������, ��� ��� �������� ��� ��������������, ��� �������� ���� �� ���������. 
� ��������� ������, ��������� �� ������ ��������� ��������.