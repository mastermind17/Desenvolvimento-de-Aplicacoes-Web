import React from 'react';
import fetch from 'universal-fetch';

import DateRangeComponent from './DateRangeSearchComponent';
import ListComponent from "./ListComponent";
import FormComponent from "./FormComponent";

import PaginationComponent from "./PaginationComponent";
import SearchBox from "./SearchBoxComponent";
import TagWrapperComponent from './TagWrapperComponent';

import { request, createNewElement } from "./../ApiSearch";

//context indicators
const HOME_CONTEXT = Symbol('home_page');
const PROJECT_CONTEXT = Symbol('project_details');
const ISSUE_CONTEXT = Symbol('issue_details');

//class indicators
const PROJECT_CLASS = "project";
const ISSUE_CLASS = "issue";
const COMMENT_CLASS = "comment";
const TAG_CLASS = "tag";


/**
* O principal componente da aplicação. Engloba outros componentes e mantém
* estado de forma a gerir os dados que são passados a cada um deles.
*/
export default class ApplicationWrapper extends React.Component{

  constructor(props){
    super(props);
    this.state = {
      parser: props.parser,
      context: HOME_CONTEXT,
      resourceClass: PROJECT_CLASS,
      mainTitle: "List of Projects"
    }
  }

  /*
  Este método irá obter a lista inicial de projetos para
  poder modificar o estado do componente e assim, fazer
  render da lista obtida.
  */
  componentWillMount(){
    request(this.state.parser.getAllProjectsUrl(), (err, res) => {
      if (err){
        console.log(err)
        return;//TODO
      }
      this.setState({
        collection: res.collection,
        hrefRoot: res.collection.href
      });
    });
  }

  /**
  * Actualiza o estado do componente após uma
  * tentativa de obter a próxima página de uma lista.
  */
  onCollectionChanged(url){
    request(url, (err, res) => {
      if(err){
        console.log(err);
        return;
      }
      //console.log(res.collection)
      //let newState = list ?  {[list]: res.collection} : {collection: res.collection};
      this.setState({collection: res.collection});
    });
  }

  onNewItemHandler(url, newItemData, list){
    createNewElement(url, newItemData, (res) => {
      //'res' representa a resposta após tentativa de
      // criação do recurso.
      if (res.status === 400){ //erro
        alert(JSON.stringify(res["invalid-params"][0].reason))
      } else this.onCollectionChanged(url, list);
    });
  }

  

  renderTypeOfIssueBox(){
    return (
      <div>
        {this.state.collection.queries.map((elem, idx) => {
          if (elem.rel === 'filter'){
            return (
              <div key={idx} className="text-right">
                <p>
                  {elem.prompt}
                </p>
                <form
                  className="navbar-form navbar-input-group"
                  role="search">
                  <select
                    onChange={(evt) => {
                      //TODO Extrair este código daqui
                      evt.preventDefault();
                      let queryPath = "";
                      if (evt.target.value != '---'){
                        queryPath += `&${elem.data[0].name}=${evt.target.value}`;
                      }
                      //change items
                      let url = elem.href + queryPath;
                      this.onCollectionChanged(url);
                    }}
                    name={elem.name}
                    className="form-control">
                    <option>---</option>
                    <option>open</option>
                    <option>closed</option>
                  </select>
                </form>
              </div>
            )
          }
        })}

      </div>
    );
  }

  renderSearchBoxes(){
    return (
      <div>
        {this.state.collection.queries.map((elem, idx) => {
          if (elem.rel === 'search'){
            return (
              <SearchBox
                key={idx}
                onSearch = {this.onCollectionChanged.bind(this)}
                queryNode = {elem}/>
            )
          }
        })}

      </div>
    );
  }

  _onSelectedProject(url){
    request(url, (err, sirenResourceProject) => {
      if (err){
        console.log(err.message);
        return;
      }

      let newState = {
        context: PROJECT_CONTEXT,
        resourceClass: ISSUE_CLASS,
        mainTitle: sirenResourceProject.properties.name,
        project: sirenResourceProject
      }

      sirenResourceProject.entities.forEach(obj => {
        if (obj.class.find(c => c === 'issues')){
          request(obj.href, (err, issues) => {
            if (err){
              console.log(err.message)
            }else {
              newState.collection = issues.collection;
              this.setState(newState);
            }
          });
        }
        if (obj.class.find(c => c === 'tags')){
          request(obj.href, (err, collectionOfTags) => {
            if (err){
              console.log(err.message);
              return;
            }
            newState.tags= collectionOfTags.collection;
            this.setState(newState);
          });
        }
      });

    });
  }

  _onSelectedIssue(url){
    request(url, (err, res) => {
      if (err){
        console.log(err.message);
        return;
      }

      /*
      Obtem os comentários para um dado issue e executa o callback
      quando a lista estiver disponivel. Assim,  só modifica estado
      uma vez.
      */
      this._getCommentsOfIssue(res, (err, commentsCollection) => {
        if (err){
          console.log(err.message);
          return;
        }
        this.setState({
          mainTitle: res.properties.title + " #" + res.properties.id,
          context: ISSUE_CONTEXT,
          resourceClass: COMMENT_CLASS,
          issue: res,
          tags: undefined,
          collection: commentsCollection.collection
        })
      });
    });
  }

  onItemSelected(evt){
    evt.preventDefault();
    let url = evt.target.href;

    switch (this.state.context) {
      case HOME_CONTEXT:
      this._onSelectedProject(url);
      break;
      case PROJECT_CONTEXT:
      this._onSelectedIssue(url);
      break;

      default:
      console.log("Unknown Context");
    }
  }

  mapListItems(item, i){
    //TODO passar para objecto
    switch(this.state.context){
      case HOME_CONTEXT:
        return this._mapProjects(item, i);
      case PROJECT_CONTEXT:
        return this._mapIssues(item, i);
      case ISSUE_CONTEXT:
        return this._mapComments(item, i);
    }
  }

  _mapProjects(item, idx){
    let projectName = item.data[1].value;
    return (
      <strong key={idx}>
        <a
          href={item.href}
          className="list-group-item"
          onClick={this.onItemSelected.bind(this)}>
          {projectName}

        </a>
      </strong>
    )
  }

  _mapIssues(item, idx){
    let issueName = item.data[0].value;
    let stateOfIssue = item.data[1].value;
    let labelClass = "pull-right ";
    labelClass += stateOfIssue === 'open' ?
    "label label-success" : "label label-danger";

    return (
      <strong key={idx}>
        <a
          href={item.href}
          className="list-group-item"
          onClick={this.onItemSelected.bind(this)}>
          {issueName}

          <span className={labelClass}>
            {stateOfIssue}
          </span>

        </a>
      </strong>
    )
  }


  _parseDate(str) {
    var mdy = str.split(' ')[0].split('/');
    console.log(mdy)
    return new Date(mdy[2], mdy[0]-1, mdy[1]);
  }

  _daydiff(first, second) {
    let miliInOneDay = (1000*60*60*24);
    return Math.floor((second-first)/miliInOneDay);
  }


  _mapComments(item, idx){
    let days = this._daydiff(this._parseDate(item.data[0].value), new Date());
    let formatDaysMessage = 
      (ammount) => { return ammount === 0 ?  "Today" : ammount + " days ago" }; 
    return (
      <div key={idx} className="container">
        <div className="row">
          <div className="col-sm-1">
            <div className="thumbnail">
              <img src="https://ssl.gstatic.com/accounts/ui/avatar_2x.png"/>
            </div>
          </div>
          <div className="col-sm-6">
            <div className="panel panel-default">
              <div className="panel-heading">
                <strong>username goes here </strong>
                <span className="text-muted">
                  { formatDaysMessage(days) }
                </span>
              </div>
              <div className="panel-body">
                {item.data[1].value}
              </div>
            </div>
          </div>
        </div>
      </div>
    )
  }

  goBackButton(){
    return (
      <div className="text-right">
        <button
          onClick={evt => {
            request(this.state.hrefRoot, (err, res)=>{
              if (err){
                console.log(err.message);
              }else{
                this.setState({
                  context: HOME_CONTEXT,
                  collection: res.collection,
                  tags: undefined,
                  issue: undefined,
                  resourceClass: PROJECT_CLASS,
                  mainTitle: "List of Projects"
                });
              }
            });
          }}
          type="button"
          className="btn btn-info">
          Go Back
        </button>
      </div>
    );
  }


  _getCommentsOfIssue(issue, afterGettingCommentsCb){
    issue.entities.forEach((ent) => {
      if (ent.class.find(c => c === 'comments')){
        request(ent.href, afterGettingCommentsCb);
      }
    })

  }

  renderPropertiesOfResource(){
    let stateOfIssue = this.state.issue.properties.state;
    let tags = this.state.issue.properties.tags;
    console.log(this.state.issue)
    return (
      <div>
        <span className={stateOfIssue === "open" ? "label label-success" : "label label-danger"}>
          {stateOfIssue}
        </span>
        <div>
          {tags.map((tag, idx) =>
            <span
              key={idx}
              className="label label-pill label-primary">
              {tag}
            </span>
          )}
        </div>
        <div>
          <b>
            <i>
              {this.state.issue.properties.description}
            </i>
          </b>
        </div>

      </div>
    )

  }

  render(){
    if(!this.state.collection){
      return null;
    }

    return (
      <div>
        <h1>
          {this.state.mainTitle}
        </h1>

        {this.state.context === ISSUE_CONTEXT ? this.renderPropertiesOfResource() : null}

        {this.state.context != ISSUE_CONTEXT ? this.renderSearchBoxes() : null}

        {this.state.context === PROJECT_CONTEXT ? this.renderTypeOfIssueBox() : null}

        {this.state.context === ISSUE_CONTEXT ? 
          <DateRangeComponent
            onSearch = {this.onCollectionChanged.bind(this)}
            href = {this.state.collection.href}/> 
            : null}

        <ListComponent
          list={this.state.collection.items}
          mapItems={this.mapListItems.bind(this)}
          onSelectedItem={this.onItemSelected.bind(this)}/>


        <PaginationComponent
          onPageChange={this.onCollectionChanged.bind(this)}
          links={this.state.collection.links}/>

        {this.state.context != HOME_CONTEXT ? this.goBackButton() : null}


        <div className="container">
          <div className="row">
            <div className="col-xs-5">
            {this.state.issue && this.state.issue.properties.state === 'closed' ?
              null : 
              <FormComponent
                class={this.state.resourceClass}
                targetSubmitHref={this.state.collection.href}
                template={this.state.collection.template}
                onNewItemHandler={this.onNewItemHandler.bind(this)}/>
            }
            </div>
            {this.state.tags ?
              (
                <div className="col-xs-3">
                  <TagWrapperComponent
                    tags={this.state.tags}
                    template={this.state.tags.template}
                    onNewItemHandler={this.onNewItemHandler.bind(this)}/>
                </div>
              ) : null}
            </div>
          </div>
        </div>
      );

    }
  }
