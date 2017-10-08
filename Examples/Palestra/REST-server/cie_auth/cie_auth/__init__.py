from pyramid.config import Configurator

from pyramid.request import Request
from pyramid.request import Response

# def request_factory(environ):
#     request = Request(environ)
#     if request.is_xhr:
#         request.response = Response()
#         request.response.headerlist = []
#         request.response.headerlist.extend(
#             (
#                 ('Access-Control-Allow-Origin', '*'),
#                 ('Content-Type', 'application/json')
#             )
#         )
#     return request


def main(global_config, **settings):
    """ This function returns a Pyramid WSGI application.
    """
    config = Configurator(settings=settings)
    # config.set_request_factory(request_factory)

    config.include('pyramid_jinja2')
    config.include('.models')
    config.include('.routes')
    config.scan()


    return config.make_wsgi_app()
