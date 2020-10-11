using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.Data.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BookStore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private ILoggerService _logger;
        private IMapper _mapper;
        public BooksController(IBookRepository BookRepository, ILoggerService logger, IMapper mapper )
        {
            _bookRepository = BookRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Books
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted call");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.LogInfo($"{location}: Successfully retrieved");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{location}: {ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get A Specific Book
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Attempted Call for id: {id}");
                var Book = await _bookRepository.FindById(id);
                
                if(Book == null)
                {
                    _logger.LogWarn($"{location}: Failed to retrieve for id: {id} ");
                    return NotFound();
                }

                var response = _mapper.Map<BookDTO>(Book);
                _logger.LogInfo($"{location}: Successfully got record with id: {id}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }

        /// <summary>
        /// Create an Book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Create attempted");
                if (bookDTO == null)
                {
                    _logger.LogWarn($"{location}: Empty request was submitted");
                    return BadRequest(ModelState);
                }

                if (bookDTO == null)
                {
                    _logger.LogWarn($"{location}: Data was incomplete");
                    return NotFound();
                }

                if(!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Data was incomplete");
                    return BadRequest(ModelState);
                }
                
                var book = _mapper.Map<Book>(bookDTO);
                
                var isSuccess = await _bookRepository.Create(book);
                
                if(!isSuccess)
                {
                    InternalError($"{location}: Creation failed");
                }
                else
                {
                    await _bookRepository.Save();
                }
                
                _logger.LogInfo($"{location}: Creation was Successfull");

                return Created("Create", new { book});
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="BookDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update( int id,[FromBody] BookUpdateDTO BookDTO)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location}: Update Attempted on record with id: {id}");
                if (id < 1 || BookDTO == null || id != BookDTO.Id)
                {
                    _logger.LogWarn($"{location}: Update Failed with bad data id: {id}");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarn($"{location}: Data was incomplete");
                    return BadRequest(ModelState);
                }
                var isExists = await _bookRepository.IsExists(id);

                if (!isExists)
                {
                    _logger.LogWarn($"{location}: Failed to retrieve record with id: {id}");
                    return NotFound();
                }
                var Book = _mapper.Map<Book>(BookDTO);

                var isSuccess = await _bookRepository.Update(Book);

                if (!isSuccess)
                {
                    InternalError($"{location}: Update failed with id: {id}");
                }
               

                _logger.LogInfo("{location}: Successfully updated an Book");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }
        ///<summary>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var location = GetControllerActionNames();
            try
            {
                _logger.LogInfo($"{location} delete attempted on record: {id} ");
                if (id < 1 )
                {
                    _logger.LogWarn($"{location} delete failed on record: {id} ");
                    return BadRequest(ModelState);
                }

                var Book = await _bookRepository.FindById(id);

                if (Book == null)
                {
                    _logger.LogWarn($"{location} delete failed with bad data on record with id: {id} ");
                    return NotFound(); 
                }
                var isSuccess = await _bookRepository.Delete(Book);
                if (!isSuccess)
                {
                    InternalError($"{location} delete failed on record with id: {id} ");
                }
                _logger.LogInfo("Successfully deleted book id: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                return InternalError($"{ex.Message} - {ex.InnerException}");
            }
        }
        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;
            return $"{controller} - {action}";
        }
        private ObjectResult InternalError(string message)
        {
            _logger.LogError($"{message}");
            return StatusCode(500, "Something went wrong. Please contact the Administrator");
        }
    }
}
