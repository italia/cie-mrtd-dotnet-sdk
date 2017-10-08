from pyramid.response import Response
from pyramid.view import view_config

from sqlalchemy.exc import DBAPIError

from ..models import MyModel
from ..models import Member

import datetime
# ISO 14443 reader

@view_config(route_name='home', renderer='../templates/mytemplate.jinja2')
def my_view(request):
    try:
        query = request.dbsession.query(MyModel)
        one = query.filter(MyModel.name == 'one').first()
    except DBAPIError:
        return Response(db_err_msg, content_type='text/plain', status=500)
    return {'one': one, 'project': 'cie_auth'}


@view_config(route_name='card', renderer='json')
def view_card(request):
    nis = request.params.get("nis")
    can = request.params.get("can")

    request.response.headers.update({
        'Access-Control-Allow-Origin': '*',
        'Access-Control-Allow-Methods': 'POST,GET,DELETE,PUT,OPTIONS',
        'Access-Control-Allow-Headers': 'Origin, Content-Type, Accept, Authorization',
        'Access-Control-Allow-Credentials': 'true',
        'Access-Control-Max-Age': '1728000',
        })

    if can is None:
        return {'nis': "AAA", 'status': "OK", 'Error': ''}

    if can != "123456":
        return {"status":"KO", "error":"CAN non valido"}
    try:
        query = request.dbsession.query(Member)
        one = query.filter(Member.id_member == 1).first()
    except DBAPIError:
        return Response(db_err_msg, content_type='text/plain', status=500)
    print("DATAAAAAAAAAAAAAA", one.birth_date_member)
    # daa = datetime.date()
    return {
            'nis': one.nis_member,
            'surname': one.surname_member,
            'name': one.name_member,
            'birth_date': "1980-12-23",
            'birth_place': one.birth_place_member,
            'birth_prov': one.birth_prov_member,
            'fiscal_code': one.fiscal_code_member,
            'res_addr': one.res_addr_member,
            'res_place': one.res_place_member,
            'res_prov': one.res_prov_member,
            "status": "OK", "error": ""
            }


@view_config(route_name='verify', renderer='json')
def view_verify(request):
    aaa = request.matchdict["nis"]

    try:
        query = request.dbsession.query(Member)
        one = query.filter(Member.nis_member == aaa).first()
    except DBAPIError:
        return Response(db_err_msg, content_type='text/plain', status=500)
    request.response.headers.update({
        'Access-Control-Allow-Origin': '*',
        'Access-Control-Allow-Methods': 'POST,GET,DELETE,PUT,OPTIONS',
        'Access-Control-Allow-Headers': 'Origin, Content-Type, Accept, Authorization',
        'Access-Control-Allow-Credentials': 'true',
        'Access-Control-Max-Age': '1728000',
        })
    if one is None:
        return {'surname': None, 'name': None, 'status': "KO", 'Error': 'Codice Utente non presente in archivio'}
    return {'surname': one.surname_member, 'name': one.name_member, 'status': "OK", 'Error': ''}


@view_config(route_name='write', renderer='json')
def view_write(request):
    nis = request.params.get("nis")
    surname = request.params.get("surname")
    name = request.params.get("name")
    birth_date = request.params.get("birth_date")
    birth_place = request.params.get("birth_place")
    birth_prov = request.params.get("birth_prov")
    fiscal_code = request.params.get("fiscal_code")
    res_addr = request.params.get("res_addr")
    res_place = request.params.get("res_place")
    res_prov = request.params.get("res_prov")

    request.response.headers.update({
        'Access-Control-Allow-Origin': '*',
        'Access-Control-Allow-Methods': 'POST,GET,DELETE,PUT,OPTIONS',
        'Access-Control-Allow-Headers': 'Origin, Content-Type, Accept, Authorization',
        'Access-Control-Allow-Credentials': 'true',
        'Access-Control-Max-Age': '1728000',
    })

    if nis is None:
        return {'surname': surname, 'name': name, 'status': "ERROR", 'Error': 'Campo obbligatorio: Nis'}
    if surname is None:
        return {'surname': surname, 'name': name, 'status': "ERROR", 'Error': 'Campo obbligatorio: Cognome'}
    if name is None:
        return {'surname': surname, 'name': name, 'status': "ERROR", 'Error': 'Campo obbligatorio: Nome'}

    try:
        query = request.dbsession.query(Member)
        one = query.filter(Member.nis_member == nis).first()
    except DBAPIError:
        return Response(db_err_msg, content_type='text/plain', status=500)

    if one is None:
        memb = Member()
        memb.nis_member = nis
        memb.surname_member = surname
        memb.name_member = name
        memb.birth_date = None
        memb.birth_place_member = birth_place
        memb.birth_prov_member = birth_prov
        memb.fiscal_code_member = fiscal_code
        memb.res_addr_member = res_addr
        memb.res_place_member = res_place
        memb.res_prov_member = res_prov

        request.dbsession.add(memb)

        return {'nis': nis, 'surname': surname, 'name': name, 'status': "INSERTED", 'Error': ''}
    return {'surname': surname, 'name': name, 'status': "UPDATED", 'Error': ''}


db_err_msg = """\
Pyramid is having a problem using your SQL database.  The problem
might be caused by one of the following things:

1.  You may need to run the "initialize_cie_auth_db" script
    to initialize your database tables.  Check your virtual
    environment's "bin" directory for this script and try to run it.

2.  Your database server may not be running.  Check that the
    database server referred to by the "sqlalchemy.url" setting in
    your "development.ini" file is running.

After you fix the problem, please restart the Pyramid application to
try it again.
"""
