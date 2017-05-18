Для работы с программой необходимо вначале настроить Ваш сервер.

Для этого авторизуйтесь на Вашем сервере под учетной записью администратора и запустите данный скрипт:

EXEC sp_addlinkedserver
@server='Azure',
@srvproduct='',    
@provider='sqlncli',
@datasrc='tcp:stasdonserver.database.windows.net,1433',
@location='',
@provstr='',
@catalog='YandexModels'

EXEC sp_addlinkedsrvlogin 'Azure', 'FALSE', NULL, 'ReadOnlyLogin', 'ReadOnly1';

После этого зайдите в директорию GoodsReviews_Setup\Debug, запустите пакет установщика Windows GoodsReviews_Setup.msi и следуйте его инструкциям.
ВАЖНО!!!! При указании папки установки убедитесь, что она доступна для редактирования, или оставьте путь по умолчанию. 
В противном случае, программа не сможет корректно работать.